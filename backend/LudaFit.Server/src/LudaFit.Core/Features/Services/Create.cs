using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Services;

internal static class Create
{
    internal sealed record Request(
        string Name,
        string Description,
        decimal Price,
        int? DiscountId
    );

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(service => service.Name)
                .NotEmpty();

            RuleFor(service => service.Description)
                .NotEmpty();

            RuleFor(service => service.Price)
                .GreaterThan(0);

            RuleFor(service => service.DiscountId)
                .GreaterThan(0)
                .When(service => service.DiscountId.HasValue);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/services", Handle)
                .Produces(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .WithTags("Services");
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

            Result createServiceResult = await handler.HandleAsync(request, cancellationToken);

            if (createServiceResult.IsFailure)
            {
                ErrorDetails errorDetails = createServiceResult.ErrorDetails!;
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
        TimeProvider timeProvider) : IScopedType
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            Discount? discount = null;

            if (request.DiscountId.HasValue)
            {
                discount = await db.Discounts
                    .FirstOrDefaultAsync(
                        discountEntity => discountEntity.Id == request.DiscountId.Value,
                        cancellationToken
                    );

                if (discount is null)
                {
                    return Result.Failure(
                        "Знижку не знайдено",
                        HttpStatusCode.NotFound
                    );
                }

                DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

                if (currentDate > discount.DateRange.End)
                {
                    return Result.Failure("Неможливо прив'язати знижку, яка вже скінчилась");
                }
            }

            Result<Service> createServiceResult = Service.Create(
                request.Name,
                request.Description,
                request.Price,
                discount
            );

            if (createServiceResult.IsFailure)
            {
                return createServiceResult;
            }

            Service service = createServiceResult.Value!;

            await db.Services.AddAsync(service, cancellationToken);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                return Result.Failure(
                    "Послуга з такою назвою вже існує",
                    HttpStatusCode.Conflict
                );
            }

            return Result.Success();
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqliteException { SqliteErrorCode: 19 };
        }
    }
}
