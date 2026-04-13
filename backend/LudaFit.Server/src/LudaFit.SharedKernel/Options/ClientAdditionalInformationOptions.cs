namespace LudaFit.SharedKernel.Options;

public sealed record ClientAdditionalInformationOptions(
    ClientHealthOptions ClientHealth,
    ClientFoodPreferencesOptions ClientFoodPreferences,
    bool? FoodWeighing
);
