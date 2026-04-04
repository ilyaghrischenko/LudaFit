using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class DiagnosisTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenNameAndBookingAreValid()
    {
        // Arrange
        Booking booking = CreateBooking();
        const string diagnosisName = "Гіпертонія";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(diagnosisName, booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(diagnosisName);
        result.Value.Booking.Should().BeSameAs(booking);
        result.Value.BookingId.Should().Be(booking.Id);
        result.Value.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMedicinesAreValidAndContainDuplicatesWithDifferentCasing()
    {
        // Arrange
        Booking booking = CreateBooking();
        const string diagnosisName = "Інсулінорезистентність";
        IReadOnlyCollection<string> medicineNames = ["Метформін", " метформін ", "Омега-3"];

        // Act
        Result<Diagnosis> result = Diagnosis.Create(diagnosisName, booking, medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(diagnosisName);
        result.Value.Booking.Should().BeSameAs(booking);
        result.Value.Medicines.Should().HaveCount(2);
        result.Value.Medicines.Select(medicine => medicine.Name)
            .Should()
            .BeEquivalentTo(["Метформін", "Омега-3"]);
        result.Value.Medicines.Should().OnlyContain(medicine => ReferenceEquals(medicine.Diagnosis, result.Value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? diagnosisName)
    {
        // Arrange
        Booking booking = CreateBooking();
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(diagnosisName!, booking);

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
    public void CreateWithMedicines_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? diagnosisName)
    {
        // Arrange
        Booking booking = CreateBooking();
        IReadOnlyCollection<string> medicineNames = ["Метформін"];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(diagnosisName!, booking, medicineNames);

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
    public void CreateWithMedicines_ShouldReturnFailure_WhenAnyMedicineNameIsNullOrWhiteSpace(string? medicineName)
    {
        // Arrange
        Booking booking = CreateBooking();
        IReadOnlyCollection<string> medicineNames = ["Метформін", medicineName!];
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create("Гастрит", booking, medicineNames);

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
        Diagnosis diagnosis = CreateDiagnosis();
        const string newName = "Переддіабет";

        // Act
        Result result = diagnosis.ChangeName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string currentName = "Гіпертонія";
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result result = diagnosis.ChangeName(newName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Name.Should().Be(currentName);
    }

    [Fact]
    public void AddMedicine_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string medicineName = "  Метформін  ";

        // Act
        Result result = diagnosis.AddMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.First().Name.Should().Be("Метформін");
        diagnosis.Medicines.First().Diagnosis.Should().BeSameAs(diagnosis);
        diagnosis.Medicines.First().DiagnosisId.Should().Be(diagnosis.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void AddMedicine_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? medicineName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.AddMedicine(medicineName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void AddMedicine_ShouldReturnFailure_WhenMedicineAlreadyExistsIgnoringCase()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result initialAddResult = diagnosis.AddMedicine("Метформін");
        const string duplicateName = "  метформін  ";
        const string expectedMessage = "Ліки з name:   метформін   вже існують";

        // Act
        Result result = diagnosis.AddMedicine(duplicateName);

        // Assert
        initialAddResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.First().Name.Should().Be("Метформін");
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_WhenNamesAreUniqueAfterNormalization()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        IReadOnlyCollection<string> medicineNames = ["Метформін", " метформін ", "Омега-3", "  Омега-3  ", "Цинк"];

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name)
            .Should()
            .BeEquivalentTo(["Метформін", "Омега-3", "Цинк"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void AddMedicines_ShouldReturnFailure_WhenAnyNameIsNullOrWhiteSpace(string? medicineName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        IReadOnlyCollection<string> medicineNames = ["Метформін", medicineName!];
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void AddMedicines_ShouldReturnFailure_WhenAnyNameAlreadyExistsInDiagnosis()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result initialAddResult = diagnosis.AddMedicine("Метформін");
        IReadOnlyCollection<string> medicineNames = ["Омега-3", "  метформін  "];
        const string expectedMessage = "Ліки з name: метформін вже існують";

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        initialAddResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.First().Name.Should().Be("Метформін");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnSuccess_WhenMedicineExists()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result addResult = diagnosis.AddMedicine("Метформін");
        Medicine medicine = diagnosis.Medicines.Single();

        // Act
        Result result = diagnosis.DeleteMedicine(medicine.Id);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const int missingId = 42;
        const string expectedMessage = "Ліків з id: 42 не знайдено";

        // Act
        Result result = diagnosis.DeleteMedicine(missingId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnSuccess_WhenMedicineExistsIgnoringCaseAndWhitespace()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result addResult = diagnosis.AddMedicine("Метформін");

        // Act
        Result result = diagnosis.DeleteMedicine("  метформін  ");

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string medicineName = "Нурофен";
        const string expectedMessage = "Ліків з name: Нурофен не знайдено";

        // Act
        Result result = diagnosis.DeleteMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void DeleteAllMedicines_ShouldRemoveAllMedicines()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result addFirstMedicineResult = diagnosis.AddMedicine("Метформін");
        Result addSecondMedicineResult = diagnosis.AddMedicine("Омега-3");

        // Act
        diagnosis.DeleteAllMedicines();

        // Assert
        addFirstMedicineResult.IsSuccess.Should().BeTrue();
        addSecondMedicineResult.IsSuccess.Should().BeTrue();
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnSuccess_WhenMedicineExistsAndNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result addResult = diagnosis.AddMedicine("Метформін");
        Medicine medicine = diagnosis.Medicines.Single();
        const string newName = "Глюкофаж";

        // Act
        Result result = diagnosis.ChangeMedicine(medicine.Id, newName);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Single().Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const int missingId = 77;
        const string expectedMessage = "Ліків з id: 77 не знайдено";

        // Act
        Result result = diagnosis.ChangeMedicine(missingId, "Глюкофаж");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeMedicine_ShouldReturnFailure_WhenNewNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        Result addResult = diagnosis.AddMedicine("Метформін");
        Medicine medicine = diagnosis.Medicines.Single();
        const string currentName = "Метформін";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.ChangeMedicine(medicine.Id, newName!);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Single().Name.Should().Be(currentName);
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
