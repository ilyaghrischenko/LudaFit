using System.Net;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record ClientAdditionalInformation
{
    public ClientHealth ClientHealth { get; init; } = null!;
    
    public ClientFoodPreferences ClientFoodPreferences { get; init; } = null!;
    
    public bool FoodWeighing { get; init; }

    private ClientAdditionalInformation() { }

    private ClientAdditionalInformation(
        ClientHealth clientHealth,
        ClientFoodPreferences clientFoodPreferences,
        bool foodWeighing)
    {
        ClientHealth = clientHealth;
        ClientFoodPreferences = clientFoodPreferences;
        FoodWeighing = foodWeighing;
    }
}
