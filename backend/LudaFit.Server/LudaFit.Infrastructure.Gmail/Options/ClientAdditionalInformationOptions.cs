namespace LudaFit.Infrastructure.Gmail.Options;

public sealed record ClientAdditionalInformationOptions(
    ClientHealthOptions ClientHealth,
    ClientFoodPreferencesOptions ClientFoodPreferences,
    bool? FoodWeighing
);
