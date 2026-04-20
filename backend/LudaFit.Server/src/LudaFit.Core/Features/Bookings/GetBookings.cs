using System.Linq.Expressions;
using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Bookings;

internal static class GetBookings
{
    internal sealed record Response
    {
        public required int Id { get; init; }
        public required string ServiceName { get; init; }
        public required decimal ServicePrice { get; init; }
        public required int ServiceId { get; init; }
        public required string FullName { get; init; }
        public required ClientMetricsDto ClientMetrics { get; init; }
        public required string Purpose { get; init; }
        public required string? PhysicalActivities { get; init; }
        public required ClientContactsDto ClientContacts { get; init; }
        public required ClientAdditionalInformationDto? ClientAdditionalInformation { get; init; }
        public required IReadOnlyCollection<DiagnosisDto> Diagnoses { get; init; }
    }

    internal sealed record ClientMetricsDto
    {
        public required uint Age { get; init; }
        public required uint Height { get; init; }
        public required float Weight { get; init; }
        public required uint WaistSize { get; init; }
    }

    internal sealed record ClientContactsDto
    {
        public required string PhoneNumber { get; init; }
        public required string Email { get; init; }
    }

    internal sealed record ClientAdditionalInformationDto
    {
        public required ClientHealthDto ClientHealth { get; init; }
        public required ClientFoodPreferencesDto ClientFoodPreferences { get; init; }
        public required bool FoodWeighing { get; init; }
    }

    internal sealed record ClientHealthDto
    {
        public required string? FeelingUnwellComplaints { get; init; }
        public required string? Allergies { get; init; }
        public required string? Intolerances { get; init; }
        public required string? StressAndHowYouCopeWithIt { get; init; }
        public required bool AnxietyTendency { get; init; }
    }

    internal sealed record ClientFoodPreferencesDto
    {
        public required string? FavoriteFoods { get; init; }
        public required string? UnfavoriteFoods { get; init; }
    }

    internal sealed record DiagnosisDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required IReadOnlyCollection<MedicineDto> Medicines { get; init; }
    }

    internal sealed record MedicineDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", Handle)
                .Produces<Pagination<Response>>()
                .WithTags("Booking");
        }

        //todo: написать про этот селектор заметку в обсидиан
        private static readonly Expression<Func<Booking, Response>> Selector = entity => new Response
        {
            Id = entity.Id,
            ServiceName = entity.ServiceName,
            ServicePrice = entity.ServicePrice,
            ServiceId = entity.ServiceId,
            FullName = entity.FullName,
            ClientMetrics = new ClientMetricsDto
            {
                Age = entity.ClientMetrics.Age,
                Height = entity.ClientMetrics.Height,
                Weight = entity.ClientMetrics.Weight,
                WaistSize = entity.ClientMetrics.WaistSize
            },
            Purpose = entity.Purpose,
            PhysicalActivities = entity.PhysicalActivities,
            ClientContacts = new ClientContactsDto
            {
                PhoneNumber = entity.ClientContacts.PhoneNumber,
                Email = entity.ClientContacts.Email.Address
            },
            ClientAdditionalInformation = entity.ClientAdditionalInformation == null
                ? null
                : new ClientAdditionalInformationDto
                {
                    ClientHealth = new ClientHealthDto
                    {
                        FeelingUnwellComplaints =
                            entity.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints,
                        Allergies = entity.ClientAdditionalInformation.ClientHealth.Allergies,
                        Intolerances = entity.ClientAdditionalInformation.ClientHealth.Intolerances,
                        StressAndHowYouCopeWithIt = entity.ClientAdditionalInformation.ClientHealth
                            .StressAndHowYouCopeWithIt,
                        AnxietyTendency = entity.ClientAdditionalInformation.ClientHealth.AnxietyTendency
                    },
                    ClientFoodPreferences = new ClientFoodPreferencesDto
                    {
                        FavoriteFoods = entity.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods,
                        UnfavoriteFoods = entity.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods
                    },
                    FoodWeighing = entity.ClientAdditionalInformation.FoodWeighing
                },
            Diagnoses = entity.Diagnoses
                .Select(diagnosis => new DiagnosisDto
                {
                    Id = diagnosis.Id,
                    Name = diagnosis.Name,
                    Medicines = diagnosis.Medicines
                        .Select(medicine => new MedicineDto
                        {
                            Id = medicine.Id,
                            Name = medicine.Name
                        })
                        .ToList()
                })
                .ToList()
        };

        private static async Task<IResult> Handle(
            [AsParameters] PaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            CancellationToken cancellationToken)
        {
            Pagination<Response> bookings = await db.Bookings
                .AsNoTracking()
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(
                    paginationParams,
                    Selector,
                    cancellationToken
                );

            return Results.Ok(bookings);
        }
    }
}
