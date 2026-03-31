using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record ClientFoodPreferences
{
    public string? FavoriteFoods { get; init; }
    
    public string? UnfavoriteFoods { get; init; }

    private ClientFoodPreferences() { }

    private ClientFoodPreferences(string? favoriteFoods = null, string? unfavoriteFoods = null)
    {
        FavoriteFoods = favoriteFoods;
        UnfavoriteFoods = unfavoriteFoods;
    }

    public static Result<ClientFoodPreferences> Create(string? favoriteFoods = null, string? unfavoriteFoods = null)
    {
        if (favoriteFoods is not null && string.IsNullOrWhiteSpace(favoriteFoods))
        {
            return new ErrorDetails("Улюблена іжа не може бути пустою");
        }
        
        if (unfavoriteFoods is not null && string.IsNullOrWhiteSpace(unfavoriteFoods))
        {
            return new ErrorDetails("Не улюблена іжа не може бути пустою");
        }

        return new ClientFoodPreferences(favoriteFoods, unfavoriteFoods);
    }
}
