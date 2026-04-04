using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.Entities;

public sealed class MedicineTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenNameAndDiagnosisAreValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string medicineName = "Метформін";

        // Act
        Result<Medicine> result = Medicine.Create(medicineName, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(medicineName);
        result.Value.Diagnosis.Should().BeSameAs(diagnosis);
        result.Value.DiagnosisId.Should().Be(diagnosis.Id);
    }

    [Fact]
    public void Create_ShouldPreserveName_WhenNameContainsLeadingOrTrailingWhitespace()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string medicineName = "  Метформін XR  ";

        // Act
        Result<Medicine> result = Medicine.Create(medicineName, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(medicineName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? medicineName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result<Medicine> result = Medicine.Create(medicineName!, diagnosis);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Result<Medicine> createMedicineResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createMedicineResult.Value!;
        const string newName = "Глюкофаж";

        // Act
        Result result = medicine.ChangeName(newName);

        // Assert
        createMedicineResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        medicine.Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeName_ShouldPreserveWhitespace_WhenNameContainsLeadingOrTrailingWhitespace()
    {
        // Arrange
        Result<Medicine> createMedicineResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createMedicineResult.Value!;
        const string newName = "  Глюкофаж Лонг  ";

        // Act
        Result result = medicine.ChangeName(newName);

        // Assert
        createMedicineResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        medicine.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Result<Medicine> createMedicineResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createMedicineResult.Value!;
        const string currentName = "Метформін";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = medicine.ChangeName(newName!);

        // Assert
        createMedicineResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        medicine.Name.Should().Be(currentName);
    }

    private static Diagnosis CreateDiagnosis()
    {
        Result<Diagnosis> diagnosisResult = Diagnosis.Create("Гіпертонія", CreateBooking());

        diagnosisResult.IsSuccess.Should().BeTrue();

        return diagnosisResult.Value!;
    }

    private static Booking CreateBooking()
    {
        Result<ClientMetrics> clientMetricsResult = ClientMetrics.Create(25, 180, 80.5f, 90);
        Result<ClientContacts> clientContactsResult = ClientContacts.Create("+380671112233", "ivan.petrenko@example.com");
        Result<Booking> bookingResult = Booking.Create(
            "Консультація",
            1500m,
            7,
            "Іван Петренко",
            clientMetricsResult.Value!,
            "Схуднення",
            clientContactsResult.Value!);

        clientMetricsResult.IsSuccess.Should().BeTrue();
        clientContactsResult.IsSuccess.Should().BeTrue();
        bookingResult.IsSuccess.Should().BeTrue();

        return bookingResult.Value!;
    }
}
