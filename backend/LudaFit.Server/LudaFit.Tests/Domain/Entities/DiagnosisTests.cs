using System.Net;
using System.Reflection;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class DiagnosisTests
{
    [Theory]
    [InlineData("Грип")]
    [InlineData("  Migraine  ")]
    public void Create_ShouldReturnSuccess_WhenNameIsValid(string name)
    {
        // Arrange

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Medicines.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void CreateWithMedicines_ShouldReturnSuccess_WhenNameAndMedicinesAreValid()
    {
        // Arrange
        const string name = "Застуда";
        IReadOnlyCollection<string> medicineNames = ["  Парацетамол  ", "Ібупрофен", "парацетамол", string.Empty, "   "];

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name, medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Medicines.Should().HaveCount(2);
        result.Value.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Парацетамол", "Ібупрофен");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateWithMedicines_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        IReadOnlyCollection<string> medicineNames = ["Парацетамол"];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name!, medicineNames);

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
        Diagnosis diagnosis = CreateDiagnosis("Грип");
        const string newName = "Ангіна";

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
    [InlineData("\r\n")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Грип");
        const string currentName = "Грип";
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
        Diagnosis diagnosis = CreateDiagnosis("Грип");
        const string medicineName = "  Парацетамол  ";

        // Act
        Result result = diagnosis.AddMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.Single().Name.Should().Be("Парацетамол");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void AddMedicine_ShouldReturnFailure_WhenMedicineNameIsNullOrWhiteSpaceAfterTrim(string name)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Грип");
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.AddMedicine(name);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void AddMedicine_ShouldReturnConflict_WhenMedicineAlreadyExistsIgnoringCase()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол");
        const string duplicateName = "  парацетамол  ";
        const string expectedMessage = "Ліки з name:   парацетамол   вже існують";

        // Act
        Result result = diagnosis.AddMedicine(duplicateName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().ContainSingle();
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_WhenNamesContainUniqueTrimmedValues()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Нурофен");
        IReadOnlyCollection<string> names = ["  Парацетамол  ", "Ібупрофен", "парацетамол", string.Empty, "   "];

        // Act
        Result result = diagnosis.AddMedicines(names);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Нурофен", "Парацетамол", "Ібупрофен");
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_WhenAllNamesAreEmptyAfterFiltering()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Нурофен");
        IReadOnlyCollection<string> names = [string.Empty, " ", "   ", "\t"];

        // Act
        Result result = diagnosis.AddMedicines(names);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.Single().Name.Should().Be("Нурофен");
    }

    [Fact]
    public void AddMedicines_ShouldReturnConflict_WhenAnyMedicineAlreadyExists_AndShouldNotAddAnyNewMedicines()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Нурофен");
        IReadOnlyCollection<string> names = ["Парацетамол", "  нурофен  ", "Ібупрофен"];
        const string expectedMessage = "Ліки з name: нурофен вже існують";

        // Act
        Result result = diagnosis.AddMedicines(names);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.Single().Name.Should().Be("Нурофен");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnSuccess_WhenMedicineExists()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол", "Ібупрофен");
        Medicine medicineToDelete = diagnosis.Medicines.First(medicine => medicine.Name == "Ібупрофен");
        SetEntityId(medicineToDelete, 7);

        // Act
        Result result = diagnosis.DeleteMedicine(7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Парацетамол");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnNotFound_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол");
        const int medicineId = 99;
        const string expectedMessage = "Ліків з id: 99 не знайдено";

        // Act
        Result result = diagnosis.DeleteMedicine(medicineId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Should().ContainSingle();
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnSuccess_WhenMedicineExistsIgnoringCaseAndWhitespace()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол", "Ібупрофен");
        const string medicineName = "  іБУпроФЕН  ";

        // Act
        Result result = diagnosis.DeleteMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name).Should().Equal("Парацетамол");
    }

    [Fact]
    public void DeleteMedicineByName_ShouldReturnNotFound_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол");
        const string medicineName = "Анальгін";
        const string expectedMessage = "Ліків з name: Анальгін не знайдено";

        // Act
        Result result = diagnosis.DeleteMedicine(medicineName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Should().ContainSingle();
    }

    [Fact]
    public void DeleteAllMedicines_ShouldClearAllMedicines()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол", "Ібупрофен");

        // Act
        diagnosis.DeleteAllMedicines();

        // Assert
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnSuccess_WhenMedicineExistsAndNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол", "Ібупрофен");
        Medicine medicineToChange = diagnosis.Medicines.First(medicine => medicine.Name == "Парацетамол");
        SetEntityId(medicineToChange, 3);
        const string newName = "  Анальгін  ";

        // Act
        Result result = diagnosis.ChangeMedicine(3, newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.First(medicine => medicine.Id == 3).Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnFailure_WhenMedicineNameIsInvalid_AndShouldNotChangeState()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол");
        Medicine medicineToChange = diagnosis.Medicines.Single();
        SetEntityId(medicineToChange, 5);
        const string invalidName = "   ";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.ChangeMedicine(5, invalidName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Single().Name.Should().Be("Парацетамол");
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnNotFound_WhenMedicineDoesNotExist()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosisWithMedicines("Грип", "Парацетамол");
        const int medicineId = 42;
        const string newName = "Анальгін";
        const string expectedMessage = "Ліків з id: 42 не знайдено";

        // Act
        Result result = diagnosis.ChangeMedicine(medicineId, newName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.NotFound);
        diagnosis.Medicines.Single().Name.Should().Be("Парацетамол");
    }

    private static Diagnosis CreateDiagnosis(string name)
    {
        Result<Diagnosis> result = Diagnosis.Create(name);
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static Diagnosis CreateDiagnosisWithMedicines(string name, params string[] medicineNames)
    {
        Result<Diagnosis> result = Diagnosis.Create(name, medicineNames);
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static void SetEntityId(Medicine medicine, int id)
    {
        FieldInfo? backingField = typeof(LudaFit.Domain.Entities.Common.BaseEntity)
            .GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        backingField.Should().NotBeNull();
        backingField!.SetValue(medicine, id);
    }
}
