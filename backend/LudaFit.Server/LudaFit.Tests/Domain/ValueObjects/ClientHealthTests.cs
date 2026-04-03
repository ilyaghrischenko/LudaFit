using System.Net;
using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.ValueObjects;

public sealed class ClientHealthTests
{
    [Theory]
    [InlineData(false, null, null, null, null, null)]
    [InlineData(true, "Headache", null, null, null, null)]
    [InlineData(false, null, "Pollen", "Lactose", "Yoga", "Breathing exercises")]
    [InlineData(true, "  Dizzy  ", "  Dust  ", "  Gluten  ", "  Running  ", "  Meditation  ")]
    public void Create_ShouldReturnSuccess_WhenValuesAreNullOrNonWhitespace(
        bool anxietyTendency,
        string? feelingUnwellComplaints,
        string? allergies,
        string? intolerances,
        string? physicalActivities,
        string? stressAndHowYouCopeWithIt)
    {
        // Arrange

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency,
            feelingUnwellComplaints,
            allergies,
            intolerances,
            physicalActivities,
            stressAndHowYouCopeWithIt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.AnxietyTendency.Should().Be(anxietyTendency);
        result.Value.FeelingUnwellComplaints.Should().Be(feelingUnwellComplaints);
        result.Value.Allergies.Should().Be(allergies);
        result.Value.Intolerances.Should().Be(intolerances);
        result.Value.PhysicalActivities.Should().Be(physicalActivities);
        result.Value.StressAndHowYouCopeWithIt.Should().Be(stressAndHowYouCopeWithIt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldReturnFailure_WhenFeelingUnwellComplaintsIsEmptyOrWhitespace(string feelingUnwellComplaints)
    {
        // Arrange
        const string expectedMessage = "Скарги на самопочуття не можуть бути пустими";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: false,
            feelingUnwellComplaints: feelingUnwellComplaints);

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
    public void Create_ShouldReturnFailure_WhenAllergiesIsEmptyOrWhitespace(string allergies)
    {
        // Arrange
        const string expectedMessage = "Алергії не можуть бути пустими";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: false,
            allergies: allergies);

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
    public void Create_ShouldReturnFailure_WhenIntolerancesIsEmptyOrWhitespace(string intolerances)
    {
        // Arrange
        const string expectedMessage = "Не переносимість їжі не може бути пустим";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: false,
            intolerances: intolerances);

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
    public void Create_ShouldReturnFailure_WhenPhysicalActivitiesIsEmptyOrWhitespace(string physicalActivities)
    {
        // Arrange
        const string expectedMessage = "Фізична активність не може бути пустою";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: false,
            physicalActivities: physicalActivities);

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
    public void Create_ShouldReturnFailure_WhenStressAndHowYouCopeWithItIsEmptyOrWhitespace(string stressAndHowYouCopeWithIt)
    {
        // Arrange
        const string expectedMessage = "Як справляєтесь зі стресом не може бути пустим полем";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: false,
            stressAndHowYouCopeWithIt: stressAndHowYouCopeWithIt);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeFeelingUnwellComplaintsValidation_BeforeOtherValidationErrors()
    {
        // Arrange
        const string expectedMessage = "Скарги на самопочуття не можуть бути пустими";

        // Act
        Result<ClientHealth> result = ClientHealth.Create(
            anxietyTendency: true,
            feelingUnwellComplaints: " ",
            allergies: "\t",
            intolerances: "\r\n",
            physicalActivities: string.Empty,
            stressAndHowYouCopeWithIt: " ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPreserveAnxietyTendency_WhenOptionalTextFieldsAreNull()
    {
        // Arrange
        const bool anxietyTendency = true;

        // Act
        Result<ClientHealth> result = ClientHealth.Create(anxietyTendency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AnxietyTendency.Should().BeTrue();
        result.Value.FeelingUnwellComplaints.Should().BeNull();
        result.Value.Allergies.Should().BeNull();
        result.Value.Intolerances.Should().BeNull();
        result.Value.PhysicalActivities.Should().BeNull();
        result.Value.StressAndHowYouCopeWithIt.Should().BeNull();
    }
}
