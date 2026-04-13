using System.Net;
using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.ValueObjects;

public sealed class ClientMetricsTests
{
    [Theory]
    [InlineData(1u, 1u, 0.1f, 1u)]
    [InlineData(25u, 180u, 80.5f, 90u)]
    [InlineData(100u, 250u, 300.75f, 150u)]
    public void Create_ShouldReturnSuccess_WhenMetricsAreValid(
        uint age,
        uint height,
        float weight,
        uint waistSize)
    {
        // Arrange

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(age, height, weight, waistSize);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Age.Should().Be(age);
        result.Value.Height.Should().Be(height);
        result.Value.Weight.Should().Be(weight);
        result.Value.WaistSize.Should().Be(waistSize);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(101u)]
    [InlineData(uint.MaxValue)]
    public void Create_ShouldReturnFailure_WhenAgeIsOutOfRange(uint age)
    {
        // Arrange
        string expectedMessage = $"Дуже сумніваюся що тобі {age} років";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(age, 180, 80.5f, 90);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(251u)]
    [InlineData(uint.MaxValue)]
    public void Create_ShouldReturnFailure_WhenHeightIsOutOfRange(uint height)
    {
        // Arrange
        string expectedMessage = $"Дуже сумніваюся що ти {height}см зростом";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(25, height, 80.5f, 90);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-0.01f)]
    [InlineData(-100f)]
    public void Create_ShouldReturnFailure_WhenWeightIsNotPositive(float weight)
    {
        // Arrange
        const string expectedMessage = "Вага не може бути від'ємною";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(25, 180, weight, 90);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenWaistSizeIsZero()
    {
        // Arrange
        const string expectedMessage = "Обхват талії не може бути 0";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(25, 180, 80.5f, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeAgeValidation_BeforeAllOtherValidationErrors()
    {
        // Arrange
        const uint age = 0;
        const uint height = 0;
        const float weight = -1f;
        const uint waistSize = 0;
        const string expectedMessage = "Дуже сумніваюся що тобі 0 років";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(age, height, weight, waistSize);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeHeightValidation_BeforeWeightAndWaistSizeErrors()
    {
        // Arrange
        const uint age = 25;
        const uint height = 0;
        const float weight = -1f;
        const uint waistSize = 0;
        const string expectedMessage = "Дуже сумніваюся що ти 0см зростом";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(age, height, weight, waistSize);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeWeightValidation_BeforeWaistSizeError()
    {
        // Arrange
        const string expectedMessage = "Вага не може бути від'ємною";

        // Act
        Result<ClientMetrics> result = ClientMetrics.Create(25, 180, -1f, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
