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
        const string serviceName = "Консультація";
        const decimal servicePrice = 1500m;
        const int serviceId = 12;
        const string fullName = "Іван Петренко";
        const uint age = 28;
        const uint height = 182;
        const float weight = 81.5f;
        const uint waistSize = 88;
        const string purpose = "Схуднення";
        const bool anxietyTendency = true;
        const bool foodWeighing = false;
        const string feelingUnwellComplaints = "Періодична втома";
        const string allergies = "Пилок";
        const string intolerances = "Лактоза";
        const string favoriteFoods = "Риба";
        const string unfavoriteFoods = "Печінка";
        const string physicalActivities = "Плавання";
        const string stressAndHowYouCopeWithIt = "Прогулянки";

        // Act
        Result<Booking> result = Booking.Create(
            serviceName,
            servicePrice,
            serviceId,
            fullName,
            age,
            height,
            weight,
            waistSize,
            purpose,
            anxietyTendency,
            foodWeighing,
            feelingUnwellComplaints: feelingUnwellComplaints,
            allergies: allergies,
            intolerances: intolerances,
            favoriteFoods: favoriteFoods,
            unfavoriteFoods: unfavoriteFoods,
            physicalActivities: physicalActivities,
            stressAndHowYouCopeWithIt: stressAndHowYouCopeWithIt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.ServiceName.Should().Be(serviceName);
        result.Value.ServicePrice.Should().Be(servicePrice);
        result.Value.ServiceId.Should().Be(serviceId);
        result.Value.FullName.Should().Be(fullName);
        result.Value.Purpose.Should().Be(purpose);
        result.Value.FoodWeighing.Should().Be(foodWeighing);
        result.Value.ClientMetrics.Age.Should().Be(age);
        result.Value.ClientMetrics.Height.Should().Be(height);
        result.Value.ClientMetrics.Weight.Should().Be(weight);
        result.Value.ClientMetrics.WaistSize.Should().Be(waistSize);
        result.Value.HealthQuestionnaire.AnxietyTendency.Should().Be(anxietyTendency);
        result.Value.HealthQuestionnaire.FeelingUnwellComplaints.Should().Be(feelingUnwellComplaints);
        result.Value.HealthQuestionnaire.Allergies.Should().Be(allergies);
        result.Value.HealthQuestionnaire.Intolerances.Should().Be(intolerances);
        result.Value.HealthQuestionnaire.PhysicalActivities.Should().Be(physicalActivities);
        result.Value.HealthQuestionnaire.StressAndHowYouCopeWithIt.Should().Be(stressAndHowYouCopeWithIt);
        result.Value.ClientFoodPreferences.FavoriteFoods.Should().Be(favoriteFoods);
        result.Value.ClientFoodPreferences.UnfavoriteFoods.Should().Be(unfavoriteFoods);
        result.Value.Diagnoses.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAndDiagnosesAreValid()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnosesForBooking =
        [
            new CreateDiagnosisForBooking("Гіпотиреоз", ["Йод", "Селен"]),
            new CreateDiagnosisForBooking("Анемія", ["Залізо"])
        ];

        // Act
        Result<Booking> result = Booking.Create(
            "Супровід",
            2200m,
            7,
            "Марія Іваненко",
            33,
            170,
            63.4f,
            75,
            "Покращення самопочуття",
            false,
            true,
            diagnosesForBooking,
            favoriteFoods: "Овочі");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.FoodWeighing.Should().BeTrue();
        result.Value.Diagnoses.Should().HaveCount(2);
        result.Value.Diagnoses.Select(diagnosis => diagnosis.Name)
            .Should()
            .Equal("Гіпотиреоз", "Анемія");
        result.Value.Diagnoses.First().Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Йод", "Селен");
        result.Value.Diagnoses.Last().Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Залізо");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldReturnFailure_WhenServiceNameIsNullOrWhiteSpace(string? serviceName)
    {
        // Arrange
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            serviceName!,
            1200m,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false);

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
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            servicePrice,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false);

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
        const string expectedMessage = "Айді послуши не може бути 0";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            0,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false);

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
            1200m,
            2,
            fullName!,
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false);

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
            1200m,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            purpose!,
            false,
            false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0, 180, 80, 90, "Дуже сумніваюся що тобі 0 років")]
    [InlineData(101, 180, 80, 90, "Дуже сумніваюся що тобі 101 років")]
    [InlineData(30, 0, 80, 90, "Дуже сумніваюся що ти 0см зростом")]
    [InlineData(30, 251, 80, 90, "Дуже сумніваюся що ти 251см зростом")]
    [InlineData(30, 180, 0, 90, "Вага не може бути від'ємною")]
    [InlineData(30, 180, -1, 90, "Вага не може бути від'ємною")]
    [InlineData(30, 180, 80, 0, "Обхват талії не може бути 0")]
    public void Create_ShouldReturnFailure_WhenClientMetricsAreInvalid(
        uint age,
        uint height,
        float weight,
        uint waistSize,
        string expectedMessage)
    {
        // Arrange

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            2,
            "Іван Петренко",
            age,
            height,
            weight,
            waistSize,
            "Схуднення",
            false,
            false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(" ", null, null, null, null, "Скарги на самопочуття не можуть бути пустими")]
    [InlineData(null, " ", null, null, null, "Алергії не можуть бути пустими")]
    [InlineData(null, null, " ", null, null, "Не переносимість їжі не може бути пустим")]
    [InlineData(null, null, null, " ", null, "Фізична активність не може бути пустою")]
    [InlineData(null, null, null, null, " ", "Як справляєтесь зі стресом не може бути пустим полем")]
    public void Create_ShouldReturnFailure_WhenHealthQuestionnaireIsInvalid(
        string? feelingUnwellComplaints,
        string? allergies,
        string? intolerances,
        string? physicalActivities,
        string? stressAndHowYouCopeWithIt,
        string expectedMessage)
    {
        // Arrange

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false,
            feelingUnwellComplaints: feelingUnwellComplaints,
            allergies: allergies,
            intolerances: intolerances,
            physicalActivities: physicalActivities,
            stressAndHowYouCopeWithIt: stressAndHowYouCopeWithIt);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(" ", null, "Улюблена іжа не може бути пустою")]
    [InlineData(null, " ", "Не улюблена іжа не може бути пустою")]
    public void Create_ShouldReturnFailure_WhenClientFoodPreferencesAreInvalid(
        string? favoriteFoods,
        string? unfavoriteFoods,
        string expectedMessage)
    {
        // Arrange

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false,
            favoriteFoods: favoriteFoods,
            unfavoriteFoods: unfavoriteFoods);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiagnosisNameIsInvalid_AndShouldNotCreateBooking()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnosesForBooking =
        [
            new CreateDiagnosisForBooking("Гіпотиреоз", ["Йод"]),
            new CreateDiagnosisForBooking(" ", ["Селен"])
        ];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            "Супровід",
            2200m,
            7,
            "Марія Іваненко",
            33,
            170,
            63.4f,
            75,
            "Покращення самопочуття",
            false,
            true,
            diagnosesForBooking);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldIgnoreEmptyDiagnosisMedicineNames_WhenDiagnosesAreCreated()
    {
        // Arrange
        CreateDiagnosisForBooking[] diagnosesForBooking =
        [
            new CreateDiagnosisForBooking("Гіпотиреоз", ["Йод", " ", string.Empty, "Селен"])
        ];

        // Act
        Result<Booking> result = Booking.Create(
            "Супровід",
            2200m,
            7,
            "Марія Іваненко",
            33,
            170,
            63.4f,
            75,
            "Покращення самопочуття",
            false,
            true,
            diagnosesForBooking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Diagnoses.Should().ContainSingle();
        result.Value.Diagnoses.Single().Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Йод", "Селен");
    }

    [Fact]
    public void Create_ShouldPrioritizeTopLevelValidation_BeforeNestedValueObjectsValidation()
    {
        // Arrange
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Booking> result = Booking.Create(
            string.Empty,
            0,
            0,
            string.Empty,
            0,
            0,
            0,
            0,
            string.Empty,
            false,
            false,
            feelingUnwellComplaints: " ",
            favoriteFoods: " ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeClientMetricsValidation_BeforeHealthQuestionnaireAndFoodPreferencesValidation()
    {
        // Arrange
        const string expectedMessage = "Дуже сумніваюся що тобі 0 років";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            2,
            "Іван Петренко",
            0,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false,
            feelingUnwellComplaints: " ",
            favoriteFoods: " ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeHealthQuestionnaireValidation_BeforeFoodPreferencesValidation()
    {
        // Arrange
        const string expectedMessage = "Скарги на самопочуття не можуть бути пустими";

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1200m,
            2,
            "Іван Петренко",
            30,
            180,
            80f,
            90,
            "Схуднення",
            false,
            false,
            feelingUnwellComplaints: " ",
            favoriteFoods: " ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
