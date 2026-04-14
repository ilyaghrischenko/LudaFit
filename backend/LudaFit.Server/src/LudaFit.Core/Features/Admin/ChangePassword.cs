using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Admin;

internal static class ChangePassword
{
    internal sealed record Request(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty();

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .Equal(x => x.NewPassword);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/admin/password", Handle)
                .RequireAuthorization()
                .Produces(StatusCodes.Status204NoContent)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .WithTags("Admin");
        }

        private static async Task<IResult> Handle(
            [FromBody] Request request,
            [FromServices] IValidator<Request> validator,
            [FromServices] Handler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid is false)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            string? adminIdClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(adminIdClaim, NumberStyles.None, CultureInfo.InvariantCulture, out int adminId))
            {
                return Results.Unauthorized();
            }

            Result result = await handler.HandleAsync(request, adminId, cancellationToken);

            if (result.IsFailure)
            {
                ErrorDetails errorDetails = result.ErrorDetails!;

                return errorDetails.StatusCode switch
                {
                    HttpStatusCode.BadRequest => Results.BadRequest(errorDetails.ErrorMessage),
                    HttpStatusCode.Unauthorized => Results.Unauthorized(),
                    HttpStatusCode.NotFound => Results.NotFound(errorDetails.ErrorMessage),
                    _ => Results.Problem(
                        detail: errorDetails.ErrorMessage,
                        statusCode: (int)errorDetails.StatusCode
                    )
                };
            }

            return Results.NoContent();
        }
    }

    internal sealed class Handler(
        LudaFitDbContext db,
        IPasswordHasher<Domain.Entities.Admin> passwordHasher) : IScopedType
    {
        public async Task<Result> HandleAsync(Request request, int adminId, CancellationToken cancellationToken)
        {
            Domain.Entities.Admin? admin = await db.Admins
                .FirstOrDefaultAsync(x => x.Id == adminId, cancellationToken);

            if (admin is null)
            {
                return Result.Failure(
                    "Адміністратора не знайдено",
                    HttpStatusCode.NotFound
                );
            }

            PasswordVerificationResult currentPasswordVerificationResult = passwordHasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                request.CurrentPassword
            );

            if (currentPasswordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Result.Failure("Невірний поточний пароль");
            }

            PasswordVerificationResult newPasswordVerificationResult = passwordHasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                request.NewPassword
            );

            if (newPasswordVerificationResult != PasswordVerificationResult.Failed)
            {
                return Result.Failure("Новий пароль має відрізнятися");
            }

            string newPasswordHash = passwordHasher.HashPassword(admin, request.NewPassword);

            Result changePasswordResult = admin.ChangePassword(admin.PasswordHash, request.NewPassword, newPasswordHash);

            if (changePasswordResult.IsFailure)
            {
                return changePasswordResult;
            }

            await db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
