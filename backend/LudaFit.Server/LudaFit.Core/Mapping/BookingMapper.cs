using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.SharedKernel.Options;

namespace LudaFit.Core.Mapping;

internal static class BookingMapper
{
    public static BookingOptions ToEmailOptions(this Booking booking)
        => new(
            booking.ServiceName,
            booking.ServicePrice,
            booking.ClientMetrics.Age,
            booking.ClientMetrics.Height,
            booking.ClientMetrics.Weight,
            booking.ClientMetrics.WaistSize,
            booking.Purpose,
            booking.PhysicalActivities,
            new ClientAdditionalInformationOptions(
                new ClientHealthOptions(
                    booking.ClientAdditionalInformation?.ClientHealth.FeelingUnwellComplaints,
                    booking.ClientAdditionalInformation?.ClientHealth.Allergies,
                    booking.ClientAdditionalInformation?.ClientHealth.Intolerances,
                    booking.ClientAdditionalInformation?.ClientHealth.StressAndHowYouCopeWithIt,
                    booking.ClientAdditionalInformation?.ClientHealth.AnxietyTendency
                ),
                new ClientFoodPreferencesOptions(
                    booking.ClientAdditionalInformation?.ClientFoodPreferences.FavoriteFoods,
                    booking.ClientAdditionalInformation?.ClientFoodPreferences.UnfavoriteFoods
                ),
                booking.ClientAdditionalInformation?.FoodWeighing
            ),
            [
                .. booking.Diagnoses.Select(d =>
                    new DiagnosisOptions(d.Name, [.. d.Medicines.Select(m => new MedicineOptions(m.Name))]))
            ]
        );
}
