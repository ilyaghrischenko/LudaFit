using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Page;

internal static class GetPage
{
    //todo: напистаь заметку про дто и что лучше через {...}
    internal sealed record Response
    {
        public required SpecialistDto Specialist { get; init; }
        public required CursorPagination<ServiceItemDto> Services { get; init; }
    }

    internal sealed record SpecialistDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required string PhotoUrl { get; init; }
        public required string Description { get; init; }
        public required TimeOnly WorkTimeStart { get; init; }
        public required TimeOnly WorkTimeEnd { get; init; }
    }

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
                .Select(specialist => new SpecialistDto
                {
                    Id = specialist.Id,
                    Name = specialist.Name,
                    PhotoUrl = specialist.PhotoUrl,
                    Description = specialist.Description,
                    WorkTimeStart = specialist.WorkTime.Start,
                    WorkTimeEnd = specialist.WorkTime.End
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (specialist is null)
            {
                return Results.NotFound();
            }

            DateOnly currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

            CursorPagination<ServiceItemDto> services = await db.Services
                .AsNoTracking()
                .ToCursorPagedListAsync(
                    paginationParams,
                    service => new ServiceItemDto(service.Id)
                    {
                        Name = service.Name,
                        Description = service.Description,
                        Price = service.Price,
                        FinalPrice = service.Discount != null 
                                     && service.Discount.DateRange.Start <= currentDate
                                     && service.Discount.DateRange.End >= currentDate
                            ? service.Price * (1 - service.Discount.Percent / 100m)
                            : service.Price,
                        Discount = service.Discount == null
                            ? null
                            : new DiscountDto
                            {
                                Id = service.Discount.Id,
                                Percent = service.Discount.Percent,
                                StartDate = service.Discount.DateRange.Start,
                                EndDate = service.Discount.DateRange.End,
                                IsActive = service.Discount.DateRange.Start <= currentDate 
                                           && service.Discount.DateRange.End >= currentDate
                            }
                    },
                    cancellationToken
                );

            Response response = new()
            {
                Specialist = specialist,
                Services = services
            };

            return Results.Ok(response);
        }
    }
}
