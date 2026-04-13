using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Mapping;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.Infrastructure.Gmail;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Booking;

internal static class Create
{
    internal sealed record Request(
        int ServiceId,
        string FullName,
        ClientMetricsRequest ClientMetrics,
        string Purpose,
        string? PhysicalActivities,
        ClientContactsRequest ClientContacts,
        ClientAdditionalInformationRequest? ClientAdditionalInformation,
        IReadOnlyCollection<DiagnosisRequest>? Diagnoses
    );

    internal sealed record ClientMetricsRequest(
        uint Age,
        uint Height,
        float Weight,
        uint WaistSize
    );

    internal sealed record ClientContactsRequest(
        string PhoneNumber,
        string Email,
        string? TelegramTag
    );

    internal sealed record ClientAdditionalInformationRequest(
        ClientHealthRequest ClientHealth,
        ClientFoodPreferencesRequest ClientFoodPreferences,
        bool FoodWeighing
    );

    internal sealed record ClientHealthRequest(
        string? FeelingUnwellComplaints,
        string? Allergies,
        string? Intolerances,
        string? StressAndHowYouCopeWithIt,
        bool AnxietyTendency
    );

    internal sealed record ClientFoodPreferencesRequest(
        string? FavoriteFoods,
        string? UnfavoriteFoods
    );

    internal sealed record DiagnosisRequest(
        string Name,
        IReadOnlyCollection<string>? MedicineNames
    );

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0);

            RuleFor(x => x.FullName)
                .NotEmpty();

            RuleFor(x => x.Purpose)
                .NotEmpty();

            RuleFor(x => x.ClientMetrics)
                .NotNull();

            RuleFor(x => x.ClientContacts)
                .NotNull();

            RuleFor(x => x.ClientMetrics.Age)
                .GreaterThan(0u);

            RuleFor(x => x.ClientMetrics.Height)
                .GreaterThan(0u);

            RuleFor(x => x.ClientMetrics.Weight)
                .GreaterThan(0);

            RuleFor(x => x.ClientMetrics.WaistSize)
                .GreaterThan(0u);

            RuleFor(x => x.ClientContacts.PhoneNumber)
                .NotEmpty();

            RuleFor(x => x.ClientContacts.Email)
                .NotEmpty();

            When(x => x.ClientAdditionalInformation is not null, () =>
            {
                RuleFor(x => x.ClientAdditionalInformation!.ClientHealth)
                    .NotNull();

                RuleFor(x => x.ClientAdditionalInformation!.ClientFoodPreferences)
                    .NotNull();
            });

            RuleForEach(x => x.Diagnoses!)
                .ChildRules(diagnosis =>
                {
                    diagnosis.RuleFor(x => x.Name)
                        .NotEmpty();
                })
                .When(x => x.Diagnoses is not null);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings", Handle)
                .Produces(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .WithTags("Booking");
        }

        private static async Task<IResult> Handle(
            [FromBody] Request request,
            [FromServices] IValidator<Request> validator,
            [FromServices] Handler handler,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid is false)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            Result result = await handler.HandleAsync(request, cancellationToken);

            if (result.IsFailure)
            {
                ErrorDetails errorDetails = result.ErrorDetails!;
                return ToFailureHttpResult(errorDetails);
            }

            return Results.Created();
        }

        private static IResult ToFailureHttpResult(ErrorDetails errorDetails)
        {
            return errorDetails.StatusCode switch
            {
                HttpStatusCode.BadRequest => Results.BadRequest(errorDetails.ErrorMessage),
                HttpStatusCode.NotFound => Results.NotFound(errorDetails.ErrorMessage),
                HttpStatusCode.Conflict => Results.Conflict(errorDetails.ErrorMessage),
                _ => Results.Problem(
                    detail: errorDetails.ErrorMessage,
                    statusCode: (int)errorDetails.StatusCode
                )
            };
        }
    }

    internal sealed class Handler(
        LudaFitDbContext db,
        EmailSender emailSender,
        TimeProvider timeProvider) : IScopedType
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            DateTime currentDateTime = timeProvider.GetUtcNow().UtcDateTime;

            Domain.Entities.Service? service = await db.Services
                .AsNoTracking()
                .Include(entity => entity.Discount)
                .FirstOrDefaultAsync(service => service.Id == request.ServiceId, cancellationToken);

            if (service is null)
            {
                return Result.Failure(
                    "Послугу не знайдено",
                    HttpStatusCode.NotFound
                );
            }

            Result<ClientMetrics> createClientMetricsResult = ClientMetrics.Create(
                request.ClientMetrics.Age,
                request.ClientMetrics.Height,
                request.ClientMetrics.Weight,
                request.ClientMetrics.WaistSize
            );

            if (createClientMetricsResult.IsFailure)
            {
                return createClientMetricsResult;
            }

            Result<ClientContacts> createClientContactsResult = ClientContacts.Create(
                request.ClientContacts.PhoneNumber,
                request.ClientContacts.Email,
                request.ClientContacts.TelegramTag
            );

            if (createClientContactsResult.IsFailure)
            {
                return createClientContactsResult;
            }

            Result<ClientAdditionalInformation?> createClientAdditionalInformationResult =
                CreateClientAdditionalInformation(request);

            if (createClientAdditionalInformationResult.IsFailure)
            {
                return createClientAdditionalInformationResult;
            }

            CreateDiagnosisForBooking[]? diagnoses = MapDiagnoses(request.Diagnoses);

            Result<Domain.Entities.Booking> createBookingResult = Domain.Entities.Booking.Create(
                service.Name,
                service.GetFinalPrice(DateOnly.FromDateTime(currentDateTime)),
                service.Id,
                request.FullName,
                createClientMetricsResult.Value!,
                request.Purpose,
                createClientContactsResult.Value!,
                request.PhysicalActivities,
                diagnoses,
                createClientAdditionalInformationResult.Value
            );

            if (createBookingResult.IsFailure)
            {
                return createBookingResult;
            }

            Domain.Entities.Booking booking = createBookingResult.Value!;

            await db.Bookings.AddAsync(booking, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            SendEmailOptions sendEmailOptions = new(
                booking.FullName,
                booking.ClientContacts.Email.Address,
                booking.ClientContacts.PhoneNumber,
                booking.ClientContacts.TelegramTag,
                booking.ToEmailOptions()
            );

            Result sendEmailResult = await emailSender.SendAsync(sendEmailOptions, cancellationToken);

            if (sendEmailResult.IsFailure)
            {
                UnsentEmail unsentEmail = new(currentDateTime, booking);

                await db.UnsentEmails.AddAsync(unsentEmail, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            
            return Result.Success();
        }

        private static Result<ClientAdditionalInformation?> CreateClientAdditionalInformation(Request request)
        {
            if (request.ClientAdditionalInformation is null)
            {
                return Result<ClientAdditionalInformation?>.Success(null);
            }

            Result<ClientHealth> clientHealthResult = ClientHealth.Create(
                request.ClientAdditionalInformation.ClientHealth.AnxietyTendency,
                request.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints,
                request.ClientAdditionalInformation.ClientHealth.Allergies,
                request.ClientAdditionalInformation.ClientHealth.Intolerances,
                request.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt
            );

            if (clientHealthResult.IsFailure)
            {
                return Result<ClientAdditionalInformation?>.Failure(clientHealthResult);
            }

            Result<ClientFoodPreferences> clientFoodPreferencesResult = ClientFoodPreferences.Create(
                request.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods,
                request.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods
            );

            if (clientFoodPreferencesResult.IsFailure)
            {
                return Result<ClientAdditionalInformation?>.Failure(clientFoodPreferencesResult);
            }

            ClientAdditionalInformation clientAdditionalInformation = new(
                clientHealthResult.Value!,
                clientFoodPreferencesResult.Value!,
                request.ClientAdditionalInformation.FoodWeighing
            );

            return Result<ClientAdditionalInformation?>.Success(clientAdditionalInformation);
        }

        private static CreateDiagnosisForBooking[]? MapDiagnoses(IReadOnlyCollection<DiagnosisRequest>? diagnoses)
        {
            if (diagnoses is null || diagnoses.Count == 0)
            {
                return null;
            }

            CreateDiagnosisForBooking[] mappedDiagnoses = diagnoses
                .Select(diagnosis => new CreateDiagnosisForBooking(
                    diagnosis.Name,
                    diagnosis.MedicineNames ?? []
                ))
                .ToArray();

            return mappedDiagnoses;
        }
    }
}
