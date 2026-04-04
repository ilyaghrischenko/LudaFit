using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Page;

internal static class GetPage
{
    internal sealed record Response(
        SpecialistDto Specialist,
        CursorPagination<ServiceItemDto> Services
    );

    internal sealed record SpecialistDto(
        int Id,
        string Name,
        string PhotoUrl,
        string Description,
        TimeOnly WorkTimeStart,
        TimeOnly WorkTimeEnd
    );

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
            app.MapGet("/page", Handle)
                .Produces<Response>()
                .ProducesProblem(404)
                .WithTags("Page");
        }

        private static async Task<IResult> Handle(
            [AsParameters] CursorPaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            [FromServices] TimeProvider timeProvider,
            CancellationToken cancellationToken)
        {
            SpecialistDto? specialist = await db.Specialists
                .AsNoTracking()
                .Select(entity => new SpecialistDto(
                    entity.Id,
                    entity.Name,
                    entity.PhotoUrl,
                    entity.Description,
                    entity.WorkTime.Start,
                    entity.WorkTime.End
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (specialist is null)
            {
                return Results.NotFound();
            }

            DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

#pragma warning disable SA1118
            CursorPagination<ServiceItemDto> services = await db.Services
                .AsNoTracking()
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

            Response response = new(specialist, services);

            return Results.Ok(response);
        }
    }
}
