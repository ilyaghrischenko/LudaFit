using System.Net;
using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.ValueObjects;

public sealed class ClientFoodPreferencesTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("Apple", null)]
    [InlineData(null, "Broccoli")]
    [InlineData("Pizza, Pasta", "Olives, Mushrooms")]
    [InlineData("  mango  ", "  onion  ")]
    public void Create_ShouldReturnSuccess_WhenPreferencesAreNullOrNonWhitespace(
        string? favoriteFoods,
        string? unfavoriteFoods)
    {
        // Arrange

        // Act
        Result<ClientFoodPreferences> result = ClientFoodPreferences.Create(favoriteFoods, unfavoriteFoods);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.FavoriteFoods.Should().Be(favoriteFoods);
        result.Value.UnfavoriteFoods.Should().Be(unfavoriteFoods);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldReturnFailure_WhenFavoriteFoodsIsEmptyOrWhitespace(string favoriteFoods)
    {
        // Arrange
        const string expectedMessage = "Улюблена іжа не може бути пустою";

        // Act
        Result<ClientFoodPreferences> result = ClientFoodPreferences.Create(favoriteFoods, "Valid food");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldReturnFailure_WhenUnfavoriteFoodsIsEmptyOrWhitespace(string unfavoriteFoods)
    {
        // Arrange
        const string expectedMessage = "Не улюблена іжа не може бути пустою";

        // Act
        Result<ClientFoodPreferences> result = ClientFoodPreferences.Create("Valid food", unfavoriteFoods);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeFavoriteFoodsValidation_WhenBothPreferencesAreWhitespace()
    {
        // Arrange
        const string favoriteFoods = " ";
        const string unfavoriteFoods = "\t";
        const string expectedMessage = "Улюблена іжа не може бути пустою";

        // Act
        Result<ClientFoodPreferences> result = ClientFoodPreferences.Create(favoriteFoods, unfavoriteFoods);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
