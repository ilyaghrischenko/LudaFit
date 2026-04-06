using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = LudaFit.Domain.Entities.Service;

namespace LudaFit.Core.Features.Discount;

internal static class GetDiscounts
{
    internal sealed record Response(
        int Id,
        uint Percent,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsActive,
        IReadOnlyCollection<string> ServiceNames
    );

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

#pragma warning disable SA1118
            Pagination<Response> discounts = await db.Discounts
                .AsNoTracking()
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(
                    paginationParams,
                    entity => new Response(
                        entity.Id,
                        entity.Percent,
                        entity.DateRange.Start,
                        entity.DateRange.End,
                        entity.DateRange.Start <= currentDate
                        && entity.DateRange.End >= currentDate,
                        EF.Property<List<ServiceEntity>>(entity, "_services")
                            .Select(service => service.Name)
                            .ToList()
                    ),
                    cancellationToken
                );
#pragma warning restore SA1118

            return Results.Ok(discounts);
        }
    }
}
