using System.Net;
using Azure;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.Infrastructure.AzureBlobStorage;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Specialist;

internal static class Create
{
    internal sealed record Request(
        string Name,
        string Description,
        int StartWorkHour,
        int StartWorkMinute,
        int EndWorkHour,
        int EndWorkMinute,
        IFormFile Photo,
        List<SocialNetworkRequest>? SocialNetworks
    );

    internal sealed record SocialNetworkRequest(
        string Name,
        string Url,
        IFormFile Photo
    );

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty();

            RuleFor(x => x.Photo)
                .NotNull();

            RuleFor(x => x.Photo.Length)
                .GreaterThan(0)
                .When(x => x.Photo is not null);

            RuleForEach(x => x.SocialNetworks!)
                .ChildRules(socialNetwork =>
                {
                    socialNetwork.RuleFor(x => x.Name)
                        .NotEmpty();

                    socialNetwork.RuleFor(x => x.Url)
                        .NotEmpty();

                    socialNetwork.RuleFor(x => x.Photo)
                        .NotNull();

                    socialNetwork.RuleFor(x => x.Photo.Length)
                        .GreaterThan(0)
                        .When(x => x.Photo is not null);
                })
                .When(x => x.SocialNetworks is not null);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/specialist", Handle)
                .Accepts<Request>("multipart/form-data")
                .Produces(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .WithTags("Specialist");
        }

        private static async Task<IResult> Handle(
            [FromForm] Request request,
            [FromServices] IValidator<Request> validator,
            [FromServices] Handler handler,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid is false)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            Result createSpecialistResult = await handler.HandleAsync(request, cancellationToken);

            if (createSpecialistResult.IsFailure)
            {
                ErrorDetails errorDetails = createSpecialistResult.ErrorDetails!;
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
        BlobRepository blobRepository) : IScopedType
    {
        public async Task<Result> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            bool specialistAlreadyExists = await db.Specialists
                .AsNoTracking()
                .AnyAsync(cancellationToken);

            if (specialistAlreadyExists)
            {
                return Result.Failure(
                    "Спеціаліст вже існує",
                    HttpStatusCode.Conflict
                );
            }

            Result<TimeRange> createWorkTimeResult = TimeRange.Create(
                request.StartWorkHour,
                request.StartWorkMinute,
                request.EndWorkHour,
                request.EndWorkMinute
            );

            if (createWorkTimeResult.IsFailure)
            {
                return createWorkTimeResult;
            }

            string specialistFileExtension = Path.GetExtension(request.Photo.FileName);
            var specialistFileName = $"specialist-{Guid.NewGuid():N}{specialistFileExtension}";
            List<string> socialNetworkFileNames = [];

            try
            {
                await using Stream specialistPhotoStream = request.Photo.OpenReadStream();
                string specialistPhotoUrl = await blobRepository.AddFileAndGetUrlAsync(
                    AzureBlobContainerName.Specialist,
                    specialistFileName,
                    specialistPhotoStream,
                    cancellationToken
                );

                Result<Domain.Entities.Specialist> createSpecialistResult = Domain.Entities.Specialist.Create(
                    request.Name,
                    specialistPhotoUrl,
                    request.Description,
                    createWorkTimeResult.Value!
                );

                if (createSpecialistResult.IsFailure)
                {
                    await DeleteUploadedFilesAsync(
                        specialistFileName,
                        socialNetworkFileNames,
                        cancellationToken
                    );
                    return createSpecialistResult;
                }

                Domain.Entities.Specialist specialist = createSpecialistResult.Value!;
                Result addSocialNetworksResult = await AddSocialNetworksAsync(
                    specialist,
                    request.SocialNetworks,
                    socialNetworkFileNames,
                    cancellationToken
                );

                if (addSocialNetworksResult.IsFailure)
                {
                    await DeleteUploadedFilesAsync(
                        specialistFileName,
                        socialNetworkFileNames,
                        cancellationToken
                    );
                    return addSocialNetworksResult;
                }

                await db.Specialists.AddAsync(specialist, cancellationToken);

                try
                {
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
                {
                    await DeleteUploadedFilesAsync(
                        specialistFileName,
                        socialNetworkFileNames,
                        cancellationToken
                    );
                    return Result.Failure(
                        "Спеціаліст з таким ім'ям вже існує",
                        HttpStatusCode.Conflict
                    );
                }

                return Result.Success();
            }
            catch
            {
                await DeleteUploadedFilesAsync(
                    specialistFileName,
                    socialNetworkFileNames,
                    cancellationToken
                );
                throw;
            }
        }

        private async Task<Result> AddSocialNetworksAsync(
            Domain.Entities.Specialist specialist,
            List<SocialNetworkRequest>? socialNetworks,
            List<string> socialNetworkFileNames,
            CancellationToken cancellationToken)
        {
            if (socialNetworks is null || socialNetworks.Count == 0)
            {
                return Result.Success();
            }

            List<CreateSocialNetworkForSpecialist> createSocialNetworks = new(socialNetworks.Count);

            foreach (SocialNetworkRequest socialNetworkRequest in socialNetworks)
            {
                string socialNetworkFileExtension = Path.GetExtension(socialNetworkRequest.Photo.FileName);
                var socialNetworkFileName = $"{socialNetworkRequest.Name.Trim()}-{Guid.NewGuid():N}{socialNetworkFileExtension}";

                await using Stream socialNetworkPhotoStream = socialNetworkRequest.Photo.OpenReadStream();
                string socialNetworkPhotoUrl = await blobRepository.AddFileAndGetUrlAsync(
                    AzureBlobContainerName.SocialNetwork,
                    socialNetworkFileName,
                    socialNetworkPhotoStream,
                    cancellationToken
                );

                socialNetworkFileNames.Add(socialNetworkFileName);
                createSocialNetworks.Add(new CreateSocialNetworkForSpecialist(
                    socialNetworkRequest.Name,
                    socialNetworkRequest.Url,
                    socialNetworkPhotoUrl
                ));
            }

            Result addSocialNetworksResult = specialist.AddSocialNetworks([.. createSocialNetworks]);

            if (addSocialNetworksResult.IsFailure)
            {
                return addSocialNetworksResult;
            }

            return Result.Success();
        }

        private async Task DeleteUploadedFilesAsync(
            string specialistFileName,
            List<string> socialNetworkFileNames,
            CancellationToken cancellationToken)
        {
            try
            {
                await blobRepository.DeleteFileAsync(
                    AzureBlobContainerName.Specialist,
                    specialistFileName,
                    cancellationToken
                );
            }
            catch (RequestFailedException)
            {
            }

            foreach (string socialNetworkFileName in socialNetworkFileNames)
            {
                try
                {
                    await blobRepository.DeleteFileAsync(
                        AzureBlobContainerName.SocialNetwork,
                        socialNetworkFileName,
                        cancellationToken
                    );
                }
                catch (RequestFailedException)
                {
                }
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqliteException { SqliteErrorCode: 19 };
        }
    }
}
