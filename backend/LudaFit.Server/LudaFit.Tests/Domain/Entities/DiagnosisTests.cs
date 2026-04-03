using System.Net;
using System.Reflection;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class DiagnosisTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenNameAndBookingAreValid()
    {
        // Arrange
        const string diagnosisName = "Гіпотиреоз";
        Booking booking = CreateBooking();

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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
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

    [Fact]
    public void Create_WithMedicines_ShouldReturnSuccess_WhenNameAndMedicinesAreValid()
    {
        // Arrange
        const string diagnosisName = "Інсулінорезистентність";
        Booking booking = CreateBooking();
        IReadOnlyCollection<string> medicineNames = [" Магній ", " ", string.Empty, "магній", "Омега-3"];

        // Act
        Result<Diagnosis> result = Diagnosis.Create(diagnosisName, booking, medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(diagnosisName);
        result.Value.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Магній", "Омега-3");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithMedicines_ShouldReturnFailure_WhenDiagnosisNameIsInvalid(string? diagnosisName)
    {
        // Arrange
        Booking booking = CreateBooking();
        IReadOnlyCollection<string> medicineNames = ["Магній"];
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

    [Fact]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        const string newName = "Анемія";

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
    [InlineData("\t")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        const string currentName = "Гіпотиреоз";
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
    public void AddMedicine_ShouldReturnSuccess_WhenMedicineNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        const string medicineName = "  Йод  ";

        // Act
        Result result = diagnosis.AddMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.Single().Name.Should().Be("Йод");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void AddMedicine_ShouldReturnFailure_WhenMedicineNameIsWhiteSpace(string medicineName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.AddMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Theory]
    [InlineData("йод")]
    [InlineData(" ЙОД ")]
    public void AddMedicine_ShouldReturnFailure_WhenMedicineAlreadyExistsIgnoringCase(string medicineName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");

        // Act
        Result result = diagnosis.AddMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.ErrorDetails!.ErrorMessage.Should().Be($"Ліки з name: {medicineName} вже існують");
        diagnosis.Medicines.Should().ContainSingle();
    }

    [Fact]
    public void AddMedicines_ShouldReturnFailure_WhenNewNamesContainExistingMedicine()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        IReadOnlyCollection<string> medicineNames = [" Йод ", "Селен", string.Empty, " ", "селен", "Магній"];

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be("Ліки з name: Йод вже існують");
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_WhenAllNewNamesAreValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        IReadOnlyCollection<string> medicineNames = [" Селен ", string.Empty, "селен", "Магній"];

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Йод", "Селен", "Магній");
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_AndDoNothing_WhenAllNamesAreBlank()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        IReadOnlyCollection<string> medicineNames = [string.Empty, " ", "   ", "\t"];

        // Act
        Result result = diagnosis.AddMedicines(medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnSuccess_WhenMedicineExists()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        diagnosis.AddMedicine("Селен");
        Medicine medicine = diagnosis.Medicines.Single(existingMedicine => existingMedicine.Name == "Селен");
        SetEntityId(medicine, 7);

        // Act
        Result result = diagnosis.DeleteMedicine(7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(existingMedicine => existingMedicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        const int missingId = 77;

        // Act
        Result result = diagnosis.DeleteMedicine(missingId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be("Ліків з id: 77 не знайдено");
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnSuccess_WhenMedicineExistsIgnoringCase()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        diagnosis.AddMedicine("Селен");

        // Act
        Result result = diagnosis.DeleteMedicine(" селен ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");

        // Act
        Result result = diagnosis.DeleteMedicine("Селен");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be("Ліків з name: Селен не знайдено");
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Fact]
    public void DeleteAllMedicines_ShouldClearAllMedicines()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        diagnosis.AddMedicine("Селен");

        // Act
        diagnosis.DeleteAllMedicines();

        // Assert
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnSuccess_WhenMedicineExistsAndNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        diagnosis.AddMedicine("Селен");
        Medicine medicine = diagnosis.Medicines.Single(existingMedicine => existingMedicine.Name == "Селен");
        SetEntityId(medicine, 15);
        const string newName = "Магній";

        // Act
        Result result = diagnosis.ChangeMedicine(15, newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(existingMedicine => existingMedicine.Name)
            .Should()
            .Equal("Йод", "Магній");
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnFailure_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");

        // Act
        Result result = diagnosis.ChangeMedicine(51, "Магній");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be("Ліків з id: 51 не знайдено");
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Йод");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ChangeMedicine_ShouldReturnFailure_WhenNewNameIsWhiteSpace(string newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз");
        diagnosis.AddMedicine("Йод");
        Medicine medicine = diagnosis.Medicines.Single();
        SetEntityId(medicine, 22);
        const string currentName = "Йод";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.ChangeMedicine(22, newName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Single().Name.Should().Be(currentName);
    }

    private static Diagnosis CreateDiagnosis(string name)
    {
        // Arrange
        Booking booking = CreateBooking();

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name, booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static Booking CreateBooking()
    {
        // Arrange

        // Act
        Result<Booking> result = Booking.Create(
            "Консультація",
            1500m,
            12,
            "Іван Петренко",
            28,
            182,
            81.5f,
            88,
            "Схуднення",
            true,
            false);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Booking booking = result.Value!;
        SetEntityId(booking, 3);
        return booking;
    }

    private static void SetEntityId(object entity, int id)
    {
        FieldInfo? fieldInfo = entity.GetType().BaseType?.GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.Should().NotBeNull();
        fieldInfo!.SetValue(entity, id);
    }
}
