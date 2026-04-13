using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Service;

internal static class GetServices
{
    internal sealed record ServiceItemDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        decimal FinalPrice,
        DiscountDto? Discount
    ) : BaseDto(Id);

    internal sealed record DiscountDto(
        int Id,
        uint Percent,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsActive
    );

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/services", Handle)
                .Produces<CursorPagination<ServiceItemDto>>()
                .WithTags("Services");
        }

        private static async Task<IResult> Handle(
            [AsParameters] CursorPaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            [FromServices] TimeProvider timeProvider,
            CancellationToken cancellationToken)
        {
            DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

#pragma warning disable SA1118
            CursorPagination<ServiceItemDto> services = await db.Services
                .AsNoTracking()
                .Include(entity => entity.Discount)
                .ToCursorPagedListAsync(
                    paginationParams,
                    entity => new ServiceItemDto(
                        entity.Id,
                        entity.Name,
                        entity.Description,
                        entity.Price,
                        entity.Discount != null
                        && entity.Discount.DateRange.Start <= currentDate
                        && entity.Discount.DateRange.End >= currentDate
                            ? entity.Price * (1 - entity.Discount.Percent / 100m)
                            : entity.Price,
                        entity.Discount == null
                            ? null
                            : new DiscountDto(
                                entity.Discount.Id,
                                entity.Discount.Percent,
                                entity.Discount.DateRange.Start,
                                entity.Discount.DateRange.End,
                                entity.Discount.DateRange.Start <= currentDate
                                && entity.Discount.DateRange.End >= currentDate
                            )
                    ),
                    cancellationToken
                );
#pragma warning restore SA1118

            return Results.Ok(services);
        }
    }
}
