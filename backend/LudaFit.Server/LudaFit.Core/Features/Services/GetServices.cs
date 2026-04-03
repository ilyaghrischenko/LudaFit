using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Domain.Enums;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Services;

internal static class GetServices
{
    internal sealed record Response(
        int Id,
        string Name,
        string Description,
        decimal Price,
        decimal PriceWithDiscount,
        int? DiscountId
    ) : BaseDto(Id);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/services", Handle)
                .WithTags("Services");
        }

        private static async Task<IResult> Handle(
            [AsParameters] CursorPaginationParams paginationParams,
            [FromServices] LudaFitDbContext context,
            CancellationToken ct)
        {
            CursorPagination<Response> response = await context.Services
                .AsNoTracking()
                .Include(service => service.Discount)
                .ToCursorPagedListAsync(
                    paginationParams,
                    service => new Response(
                        service.Id,
                        service.Name,
                        service.Description,
                        service.Price,
                        service.Discount == null || service.Discount.Status == DiscountStatus.Expired ? service.Price : service.Price * (1 - service.Discount!.Percent / 100m),
                        service.DiscountId
                    ),
                    ct
                );

            return Results.Ok(response);
        }
    }
}
