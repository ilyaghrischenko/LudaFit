using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.Entities;

public sealed class BookingTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValidWithoutDiagnoses()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        // Assert
        clientMetricsResult.IsSuccess.Should().BeTrue();
        clientContactsResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.ServiceName.Should().Be("Консультація");
        result.Value.ServicePrice.Should().Be(1500m);
        result.Value.ServiceId.Should().Be(7);
        result.Value.FullName.Should().Be("Іван Петренко");
        result.Value.ClientMetrics.Should().Be(clientMetricsResult.Value);
        result.Value.Purpose.Should().Be("Схуднення");
        result.Value.ClientContacts.Should().Be(clientContactsResult.Value);
        result.Value.PhysicalActivities.Should().BeNull();
        result.Value.ClientAdditionalInformation.Should().BeNull();
        result.Value.Diagnoses.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDiagnosesAndPhysicalActivitiesAreValid()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(30, 175, 72.4f, 82);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "client@example.com");
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Цукровий діабет", ["Метформін", " метформін "]),
            new CreateDiagnosisForBooking("Гіпотиреоз", ["L-Тироксин"])
        ];
        const string physicalActivities = "Плавання двічі на тиждень";

        // Act
        Result<Booking> result = Booking.Create(
            "Супровід",
            2200m,
            11,
            "Марія Іваненко",
            clientMetricsResult.Value!,
            "Набір здорових звичок",
            clientContactsResult.Value!,
            physicalActivities,
            diagnoses);

        // Assert
        clientMetricsResult.IsSuccess.Should().BeTrue();
        clientContactsResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PhysicalActivities.Should().Be(physicalActivities);
        result.Value.Diagnoses.Should().HaveCount(2);

        Diagnosis firstDiagnosis = result.Value.Diagnoses.First(diagnosis => diagnosis.Name == "Цукровий діабет");
        firstDiagnosis.Medicines.Should().HaveCount(1);
        firstDiagnosis.Medicines.First().Name.Should().Be("Метформін");
        firstDiagnosis.Booking.Should().BeSameAs(result.Value);

        Diagnosis secondDiagnosis = result.Value.Diagnoses.First(diagnosis => diagnosis.Name == "Гіпотиреоз");
        secondDiagnosis.Medicines.Should().ContainSingle(medicine => medicine.Name == "L-Тироксин");
        secondDiagnosis.Booking.Should().BeSameAs(result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenServiceNameIsNullOrWhiteSpace(string? serviceName)
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            serviceName!,
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_ShouldReturnFailure_WhenServicePriceIsNotPositive(decimal servicePrice)
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            servicePrice,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenServiceIdIsZero()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Айді послуги не може бути 0";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            0,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenFullNameIsNullOrWhiteSpace(string? fullName)
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "ПІБ не може бути пустим";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            fullName!,
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenPurposeIsNullOrWhiteSpace(string? purpose)
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Мета схуднення не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            purpose!,
            clientContactsResult.Value!);

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
    public void Create_ShouldReturnFailure_WhenPhysicalActivitiesIsWhiteSpace(string physicalActivities)
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Фізична активність не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!,
            physicalActivities);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiagnosesContainDuplicateNamesIgnoringCaseAndSpaces()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Інсулінорезистентність", []),
            new CreateDiagnosisForBooking("  інсулінорезистентність  ", [])
        ];
        const string expectedMessage = "Діагноз з назвою   інсулінорезистентність   вже існує";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!,
            diagnosesForBooking: diagnoses);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiagnosisNameIsInvalid()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking(" ", ["Метформін"])
        ];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!,
            diagnosesForBooking: diagnoses);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiagnosisContainsInvalidMedicineName()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Інсулінорезистентність", [string.Empty])
        ];
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!,
            diagnosesForBooking: diagnoses);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeServiceNameValidation_BeforeOtherValidationErrors()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            string.Empty,
            0,
            0,
            string.Empty,
            clientMetricsResult.Value!,
            string.Empty,
            clientContactsResult.Value!,
            " ",
            [new CreateDiagnosisForBooking(" ", [string.Empty])]);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizePhysicalActivitiesValidation_BeforeDiagnosisValidation()
    {
        // Arrange
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking(" ", ["Метформін"])
        ];
        const string expectedMessage = "Фізична активність не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!,
            " ",
            diagnoses);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
