using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Services;

internal static class GetServices
{
    internal sealed record ServiceItemDto(int Id) : BaseDto(Id)
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required decimal Price { get; init; }
        public required decimal FinalPrice { get; init; }
        public required DiscountDto? Discount { get; init; }
    }

    internal sealed record DiscountDto
    {
        public required int Id { get; init; }
        public required uint Percent { get; init; }
        public required DateOnly StartDate { get; init; }
        public required DateOnly EndDate { get; init; }
        public required bool IsActive { get; init; }
    }

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

            CursorPagination<ServiceItemDto> services = await db.Services
                .AsNoTracking()
                .Include(entity => entity.Discount)
                .ToCursorPagedListAsync(
                    paginationParams,
                    entity => new ServiceItemDto(entity.Id)
                    {
                        Name = entity.Name,
                        Description = entity.Description,
                        Price = entity.Price,
                        FinalPrice = entity.Discount != null 
                                     && entity.Discount.DateRange.Start <= currentDate
                                     && entity.Discount.DateRange.End >= currentDate 
                            ? entity.Price * (1 - entity.Discount.Percent / 100m)
                            : entity.Price,
                        Discount = entity.Discount == null 
                            ? null
                            : new DiscountDto
                            {
                                Id = entity.Discount.Id,
                                Percent = entity.Discount.Percent,
                                StartDate = entity.Discount.DateRange.Start,
                                EndDate = entity.Discount.DateRange.End,
                                IsActive = entity.Discount.DateRange.Start <= currentDate
                                && entity.Discount.DateRange.End >= currentDate
                            }
                    },
                    cancellationToken
                );

            return Results.Ok(services);
        }
    }
}
