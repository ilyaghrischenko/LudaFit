using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Discounts;

internal static class Create
{
    internal sealed record Request(
        DateOnly StartDate,
        DateOnly EndDate,
        uint Percent
    );

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(discount => discount.Percent)
                .GreaterThan(0u)
                .LessThanOrEqualTo(100u);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/discounts", Handle)
                .Produces(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .WithTags("Discount");
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

            Result createDiscountResult = await handler.HandleAsync(request, cancellationToken);

            if (createDiscountResult.IsFailure)
            {
                ErrorDetails errorDetails = createDiscountResult.ErrorDetails!;
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
            bool hasOverlappingDiscount = await db.Discounts
                .AsNoTracking()
                .AnyAsync(
                    discount => discount.DateRange.Start <= request.EndDate
                                && discount.DateRange.End >= request.StartDate,
                    cancellationToken
                );

            if (hasOverlappingDiscount)
            {
                return Result.Failure(
                    "Знижка з таким періодом вже існує",
                    HttpStatusCode.Conflict
                );
            }

            DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

            Result<DateRange> createDateRangeResult = DateRange.Create(
                currentDate,
                request.StartDate,
                request.EndDate
            );

            if (createDateRangeResult.IsFailure)
            {
                return createDateRangeResult;
            }

            Result<Discount> createDiscountResult = Discount.Create(
                createDateRangeResult.Value!,
                request.Percent
            );

            if (createDiscountResult.IsFailure)
            {
                return createDiscountResult;
            }

            Discount discount = createDiscountResult.Value!;

            await db.Discounts.AddAsync(discount, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
