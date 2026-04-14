using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Services;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Admin;

internal static class Register
{
    internal sealed record Request(string Login, string Password, string ConfirmPassword);

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(admin => admin.Login)
                .NotEmpty();

            RuleFor(admin => admin.Password)
                .NotEmpty();

            RuleFor(admin => admin.ConfirmPassword)
                .NotEmpty()
                .Equal(admin => admin.Password);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/register", Handle)
                .Produces<string>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict)
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

            Result<string> registerAdminResult = await handler.HandleAsync(request, cancellationToken);

            if (registerAdminResult.IsFailure)
            {
                return ToFailureHttpResult(registerAdminResult.ErrorDetails!);
            }

            return Results.Created("/admin/register", registerAdminResult.Value!);
        }

        private static IResult ToFailureHttpResult(ErrorDetails errorDetails)
        {
            return errorDetails.StatusCode switch
            {
                HttpStatusCode.BadRequest => Results.BadRequest(errorDetails.ErrorMessage),
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
        JwtService jwtService,
        IPasswordHasher<Domain.Entities.Admin> passwordHasher) : IScopedType
    {
        public async Task<Result<string>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            bool isLoginTaken = await db.Admins
                .AsNoTracking()
                .AnyAsync(admin => admin.Login == request.Login, cancellationToken);

            if (isLoginTaken)
            {
                return Result<string>.Failure("Логін уже зайнятий", HttpStatusCode.Conflict);
            }

            string passwordHash = passwordHasher.HashPassword((Domain.Entities.Admin)null!, request.Password);

            Result<Domain.Entities.Admin> createAdminResult = Domain.Entities.Admin.Create(
                request.Login,
                request.Password,
                passwordHash
            );

            if (createAdminResult.IsFailure)
            {
                return Result<string>.Failure(createAdminResult);
            }

            Domain.Entities.Admin admin = createAdminResult.Value!;

            await db.Admins.AddAsync(admin, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            string token = jwtService.GenerateToken(admin.Id, admin.Login);

            return token;
        }
    }
}
