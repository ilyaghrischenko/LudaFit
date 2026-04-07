using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Core.Services;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Admin;

internal static class Login
{
    internal sealed record Request(string Login, string Password);

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Login)
                .NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/login", Handle)
                .Produces<string>()
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithTags("Admin");
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

            Result<string> result = await handler.HandleAsync(request, cancellationToken);
            
            if (result.IsFailure)
            {
                ErrorDetails errorDetails = result.ErrorDetails!;

                return errorDetails.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => Results.Unauthorized(),
                    HttpStatusCode.BadRequest => Results.BadRequest(errorDetails.ErrorMessage),
                    _ => Results.Problem(
                        detail: errorDetails.ErrorMessage,
                        statusCode: (int)errorDetails.StatusCode
                    )
                };
            }

            return Results.Ok(result.Value!);
        }
    }

    internal sealed class Handler(
        LudaFitDbContext db,
        JwtService jwtService,
        IPasswordHasher<Domain.Entities.Admin> passwordHasher) : IScopedType
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            Domain.Entities.Admin? admin = await db.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken);

            if (admin is null)
            {
                return Result<string>.Failure(
                    "Невірний логін або пароль",
                    HttpStatusCode.Unauthorized
                );
            }

            PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Result<string>.Failure(
                    "Невірний логін або пароль",
                    HttpStatusCode.Unauthorized
                );
            }

            string token = jwtService.GenerateToken(admin.Id, admin.Login);

            return Result<string>.Success(token);
        }
    }
}
