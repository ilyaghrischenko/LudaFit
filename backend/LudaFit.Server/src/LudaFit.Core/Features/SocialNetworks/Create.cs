using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.AzureBlobStorage;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.SocialNetworks;

internal static class Create
{
    internal sealed record Request(
        int SpecialistId,
        string Name,
        string Url,
        IFormFile Photo
    );

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SpecialistId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Url)
                .NotEmpty();

            RuleFor(x => x.Photo)
                .NotNull();

            RuleFor(x => x.Photo.Length)
                .GreaterThan(0)
                .When(x => x.Photo is not null);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/social-networks", Handle)
                .Accepts<Request>("multipart/form-data")
                .Produces(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .WithTags("SocialNetwork");
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

            Result createSocialNetworkResult = await handler.HandleAsync(request, cancellationToken);

            if (createSocialNetworkResult.IsFailure)
            {
                ErrorDetails errorDetails = createSocialNetworkResult.ErrorDetails!;
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
            Specialist? specialist = await db.Specialists
                .Include("_socialNetworks")
                .FirstOrDefaultAsync(
                    specialist => specialist.Id == request.SpecialistId,
                    cancellationToken
                );

            if (specialist is null)
            {
                return Result.Failure(
                    "Спеціаліста не знайдено",
                    HttpStatusCode.NotFound
                );
            }

            string fileExtension = Path.GetExtension(request.Photo.FileName);
            var fileName = $"{request.SpecialistId}-{Guid.NewGuid():N}{fileExtension}";

            await using Stream photoStream = request.Photo.OpenReadStream();
            string photoUrl = await blobRepository.AddFileAndGetUrlAsync(
                AzureBlobContainerName.SocialNetwork,
                fileName,
                photoStream,
                cancellationToken
            );

            Result addSocialNetworkResult = specialist.AddSocialNetwork(
                request.Name,
                request.Url,
                photoUrl
            );

            if (addSocialNetworkResult.IsFailure)
            {
                await blobRepository.DeleteFileAsync(
                    AzureBlobContainerName.SocialNetwork,
                    fileName,
                    cancellationToken
                );
                return addSocialNetworkResult;
            }

            await db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
