using System.Linq.Expressions;
using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Discounts;

internal static class GetDiscounts
{
    internal sealed record Response
    {
        public required int Id { get; init; }
        public required uint Percent { get; init; }
        public required DateOnly StartDate { get; init; }
        public required DateOnly EndDate { get; init; }
        public required bool IsActive { get; init; }
        public required IReadOnlyCollection<string> ServiceNames { get; init; }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/discounts", Handle)
                .Produces<Pagination<Response>>()
                .WithTags("Discount");
        }

        private static async Task<IResult> Handle(
            [AsParameters] PaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            [FromServices] TimeProvider timeProvider,
            CancellationToken cancellationToken)
        {
            DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

            Pagination<Response> discounts = await db.Discounts
                .AsNoTracking()
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(
                    paginationParams,
                    discount => new Response
                    {
                        Id = discount.Id,
                        Percent = discount.Percent,
                        StartDate = discount.DateRange.Start,
                        EndDate = discount.DateRange.End,
                        IsActive = discount.DateRange.Start <= currentDate
                        && discount.DateRange.End >= currentDate,
                        ServiceNames = discount.Services
                            .Select(service => service.Name)
                            .ToList()
                    },
                    cancellationToken
                );

            return Results.Ok(discounts);
        }
    }
}
