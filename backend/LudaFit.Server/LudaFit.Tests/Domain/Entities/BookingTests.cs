using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class BookingTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid_AndDiagnosesAreNotProvided()
    {
        // Arrange

        // Act
        Result<Booking> result = CreateValidBooking();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.ServiceName.Should().Be("Консультація");
        result.Value.ServicePrice.Should().Be(1500m);
        result.Value.ServiceId.Should().Be(7);
        result.Value.FullName.Should().Be("Іван Петренко");
        result.Value.Purpose.Should().Be("Схуднення");
        result.Value.FoodWeighing.Should().BeTrue();
        result.Value.ClientMetrics.Age.Should().Be(30u);
        result.Value.ClientMetrics.Height.Should().Be(180u);
        result.Value.ClientMetrics.Weight.Should().Be(82.5f);
        result.Value.ClientMetrics.WaistSize.Should().Be(92u);
        result.Value.ClientHealth.AnxietyTendency.Should().BeTrue();
        result.Value.ClientHealth.FeelingUnwellComplaints.Should().Be("Втома ввечері");
        result.Value.ClientHealth.Allergies.Should().Be("Пилок");
        result.Value.ClientHealth.Intolerances.Should().Be("Лактоза");
        result.Value.ClientHealth.PhysicalActivities.Should().Be("Ходьба");
        result.Value.ClientHealth.StressAndHowYouCopeWithIt.Should().Be("Медитація");
        result.Value.ClientFoodPreferences.FavoriteFoods.Should().Be("Риба");
        result.Value.ClientFoodPreferences.UnfavoriteFoods.Should().Be("Печінка");
        result.Value.ClientContacts.PhoneNumber.Should().Be("+380671112233");
        result.Value.ClientContacts.Email.Address.Should().Be("ivan.petrenko@example.com");
        result.Value.Diagnoses.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid_AndDiagnosesAreProvided()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Інсулінорезистентність", ["Метформін", "Вітамін D"]),
            new CreateDiagnosisForBooking("Анемія", ["Феритин"])
        ];

        // Act
        Result<Booking> result = CreateValidBooking(diagnoses);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Diagnoses.Should().HaveCount(2);
        result.Value.Diagnoses.Select(diagnosis => diagnosis.Name)
            .Should()
            .Equal("Інсулінорезистентність", "Анемія");
        result.Value.Diagnoses.First().Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Метформін", "Вітамін D");
        result.Value.Diagnoses.Last().Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Феритин");
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDiagnosesCollectionIsEmpty()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnoses = [];

        // Act
        Result<Booking> result = CreateValidBooking(diagnoses);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Diagnoses.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenServiceNameIsNullOrWhiteSpace(string? serviceName)
    {
        // Arrange
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            serviceName!,
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

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
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_ShouldReturnFailure_WhenServicePriceIsZeroOrLess(decimal servicePrice)
    {
        // Arrange
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            servicePrice,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

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
        const string expectedMessage = "Айді послуги не може бути 0";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            0,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

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
        const string expectedMessage = "ПІБ не може бути пустим";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            fullName!,
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

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
        const string expectedMessage = "Мета схуднення не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            purpose!,
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        const string expectedMessage = "Номер телефону вказаний не вірно";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "12345",
            "ivan.petrenko@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        // Arrange
        const string expectedMessage = "Пошта вказана не вірно";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "invalid-email");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClientMetricsAreInvalid()
    {
        // Arrange
        const string expectedMessage = "Дуже сумніваюся що тобі 0 років";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            0,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClientHealthIsInvalid()
    {
        // Arrange
        const string expectedMessage = "Скарги на самопочуття не можуть бути пустими";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com",
            feelingUnwellComplaints: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenClientFoodPreferencesAreInvalid()
    {
        // Arrange
        const string expectedMessage = "Улюблена іжа не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com",
            favoriteFoods: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiagnosesContainDuplicateNamesIgnoringCaseAndWhitespace()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Інсулінорезистентність", ["Метформін"]),
            new CreateDiagnosisForBooking("  інсулінорезистентність  ", ["Вітамін D"])
        ];
        const string expectedMessage = "Діагноз з назвою   інсулінорезистентність   вже існує";

        // Act
        Result<Booking> result = CreateValidBooking(diagnoses);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenDiagnosisNameIsNullOrWhiteSpace(string diagnosisName)
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking(diagnosisName, ["Метформін"])
        ];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Booking> result = CreateValidBooking(diagnoses);

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
    public void Create_ShouldIgnoreMedicineNames_WhenTheyAreNullOrWhiteSpace(string medicineName)
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnoses =
        [
            new CreateDiagnosisForBooking("Інсулінорезистентність", [medicineName])
        ];

        // Act
        Result<Booking> result = CreateValidBooking(diagnoses);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Diagnoses.Should().HaveCount(1);
        result.Value.Diagnoses.Single().Medicines.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldPrioritizeServiceNameValidation_BeforeAllNestedValidationErrors()
    {
        // Arrange
        const string serviceName = "";
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            serviceName,
            0m,
            0,
            string.Empty,
            0,
            0,
            -1f,
            0,
            string.Empty,
            true,
            true,
            string.Empty,
            string.Empty,
            feelingUnwellComplaints: string.Empty,
            favoriteFoods: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeClientContactsValidation_BeforeClientMetricsValidation()
    {
        // Arrange
        const string expectedMessage = "Номер телефону вказаний не вірно";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            0,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "invalid-phone",
            "ivan.petrenko@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeClientMetricsValidation_BeforeClientHealthValidation()
    {
        // Arrange
        const string expectedMessage = "Дуже сумніваюся що тобі 0 років";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            0,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com",
            feelingUnwellComplaints: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeClientHealthValidation_BeforeClientFoodPreferencesValidation()
    {
        // Arrange
        const string expectedMessage = "Скарги на самопочуття не можуть бути пустими";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com",
            feelingUnwellComplaints: string.Empty,
            favoriteFoods: string.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static Result<Booking> CreateValidBooking(CreateDiagnosisForBooking[]? diagnoses = null)
        => Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            30,
            180,
            82.5f,
            92,
            "Схуднення",
            true,
            true,
            "+380671112233",
            "ivan.petrenko@example.com",
            diagnoses,
            "Втома ввечері",
            "Пилок",
            "Лактоза",
            "Риба",
            "Печінка",
            "Ходьба",
            "Медитація");
}
