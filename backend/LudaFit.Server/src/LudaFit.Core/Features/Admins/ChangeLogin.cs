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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Admins;

internal static class ChangeLogin
{
    internal sealed record Request(string NewLogin, string ConfirmNewLogin);

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.NewLogin)
                .NotEmpty();

            RuleFor(x => x.ConfirmNewLogin)
                .NotEmpty()
                .Equal(x => x.NewLogin);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/admin/login", Handle)
                .RequireAuthorization()
                .Produces(StatusCodes.Status204NoContent)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status409Conflict)
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
                    HttpStatusCode.Conflict => Results.Conflict(errorDetails.ErrorMessage),
                    _ => Results.Problem(
                        detail: errorDetails.ErrorMessage,
                        statusCode: (int)errorDetails.StatusCode
                    )
                };
            }

            return Results.NoContent();
        }
    }

    internal sealed class Handler(LudaFitDbContext db) : IScopedType
    {
        public async Task<Result> HandleAsync(Request request, int adminId, CancellationToken cancellationToken)
        {
            Domain.Entities.Admin? admin = await db.Admins
                .FirstOrDefaultAsync(x => x.Id == adminId, cancellationToken);

            if (admin is null)
            {
                return Result.Failure("Адміністратора не знайдено", HttpStatusCode.NotFound);
            }

            bool isLoginTaken = await db.Admins
                .AsNoTracking()
                .AnyAsync(x => x.Login == request.NewLogin && x.Id != admin.Id, cancellationToken);

            if (isLoginTaken)
            {
                return Result.Failure("Логін уже зайнятий", HttpStatusCode.Conflict);
            }

            Result changeLoginResult = admin.ChangeLogin(request.NewLogin);

            if (changeLoginResult.IsFailure)
            {
                return changeLoginResult;
            }

            await db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
