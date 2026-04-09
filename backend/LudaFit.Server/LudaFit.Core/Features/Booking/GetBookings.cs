using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Interfaces;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.Booking;

internal static class GetBookings
{
    internal sealed record Response(
        int Id,
        string ServiceName,
        decimal ServicePrice,
        int ServiceId,
        string FullName,
        ClientMetricsDto ClientMetrics,
        string Purpose,
        string? PhysicalActivities,
        ClientContactsDto ClientContacts,
        ClientAdditionalInformationDto? ClientAdditionalInformation,
        IReadOnlyCollection<DiagnosisDto> Diagnoses
    );

    internal sealed record ClientMetricsDto(
        uint Age,
        uint Height,
        float Weight,
        uint WaistSize
    );

    internal sealed record ClientContactsDto(
        string PhoneNumber,
        string Email
    );

    internal sealed record ClientAdditionalInformationDto(
        ClientHealthDto ClientHealth,
        ClientFoodPreferencesDto ClientFoodPreferences,
        bool FoodWeighing
    );

    internal sealed record ClientHealthDto(
        string? FeelingUnwellComplaints,
        string? Allergies,
        string? Intolerances,
        string? StressAndHowYouCopeWithIt,
        bool AnxietyTendency
    );

    internal sealed record ClientFoodPreferencesDto(
        string? FavoriteFoods,
        string? UnfavoriteFoods
    );

    internal sealed record DiagnosisDto(
        int Id,
        string Name,
        IReadOnlyCollection<MedicineDto> Medicines
    );

    internal sealed record MedicineDto(
        int Id,
        string Name
    );

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", Handle)
                .Produces<Pagination<Response>>()
                .WithTags("Booking");
        }

        private static async Task<IResult> Handle(
            [AsParameters] PaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            CancellationToken cancellationToken)
        {
#pragma warning disable SA1118
            Pagination<Response> bookings = await db.Bookings
                .AsNoTracking()
                .Include("_diagnoses._medicines")
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(
                    paginationParams,
                    entity => new Response(
                        entity.Id,
                        entity.ServiceName,
                        entity.ServicePrice,
                        entity.ServiceId,
                        entity.FullName,
                        new ClientMetricsDto(
                            entity.ClientMetrics.Age,
                            entity.ClientMetrics.Height,
                            entity.ClientMetrics.Weight,
                            entity.ClientMetrics.WaistSize
                        ),
                        entity.Purpose,
                        entity.PhysicalActivities,
                        new ClientContactsDto(
                            entity.ClientContacts.PhoneNumber,
                            entity.ClientContacts.Email.Address
                        ),
                        entity.ClientAdditionalInformation == null
                            ? null
                            : new ClientAdditionalInformationDto(
                                new ClientHealthDto(
                                    entity.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints,
                                    entity.ClientAdditionalInformation.ClientHealth.Allergies,
                                    entity.ClientAdditionalInformation.ClientHealth.Intolerances,
                                    entity.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt,
                                    entity.ClientAdditionalInformation.ClientHealth.AnxietyTendency
                                ),
                                new ClientFoodPreferencesDto(
                                    entity.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods,
                                    entity.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods
                                ),
                                entity.ClientAdditionalInformation.FoodWeighing
                            ),
                        EF.Property<List<Diagnosis>>(entity, "_diagnoses")
                            .Select(diagnosis => new DiagnosisDto(
                                diagnosis.Id,
                                diagnosis.Name,
                                EF.Property<List<Medicine>>(diagnosis, "_medicines")
                                    .Select(medicine => new MedicineDto(
                                        medicine.Id,
                                        medicine.Name
                                    ))
                                    .ToList()
                            ))
                            .ToList()
                    ),
                    cancellationToken
                );
#pragma warning restore SA1118

            return Results.Ok(bookings);
        }
    }
}
