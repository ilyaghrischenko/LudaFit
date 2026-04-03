using System.Net;
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
        Booking booking = CreateValidBooking();
        const string name = "Інсулінорезистентність";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name, booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Booking.Should().BeSameAs(booking);
        result.Value.BookingId.Should().Be(booking.Id);
        result.Value.Medicines.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        Booking booking = CreateValidBooking();
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name!, booking);

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
        Booking booking = CreateValidBooking();
        IReadOnlyCollection<string> medicineNames = [" Метформін ", "метформін", "  ", "Вітамін D"];

        // Act
        Result<Diagnosis> result = Diagnosis.Create("Інсулінорезистентність", booking, medicineNames);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Medicines.Should().HaveCount(2);
        result.Value.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Метформін", "Вітамін D");
        result.Value.Medicines.Should().OnlyContain(medicine => ReferenceEquals(medicine.Diagnosis, result.Value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateWithMedicines_ShouldReturnFailure_WhenDiagnosisNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        Booking booking = CreateValidBooking();
        IReadOnlyCollection<string> medicineNames = ["Метформін"];
        const string expectedMessage = "Назва діагнозу не може бути пустою";

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name!, booking, medicineNames);

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
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string currentName = "Інсулінорезистентність";
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
        Diagnosis diagnosis = CreateDiagnosis();

        // Act
        Result result = diagnosis.AddMedicine(" Метформін ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Should().HaveCount(1);
        diagnosis.Medicines.Single().Name.Should().Be("Метформін");
        diagnosis.Medicines.Single().Diagnosis.Should().BeSameAs(diagnosis);
    }

    [Fact]
    public void AddMedicine_ShouldReturnFailure_WhenMedicineAlreadyExistsIgnoringCaseAndWhitespace()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        diagnosis.AddMedicine("Метформін");
        const string expectedMessage = "Ліки з name:  метФОрмін  вже існують";

        // Act
        Result result = diagnosis.AddMedicine(" метФОрмін ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void AddMedicine_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.AddMedicine(name!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void AddMedicines_ShouldReturnSuccess_WhenNamesContainWhitespaceDuplicatesAndEmptyValues()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        IReadOnlyCollection<string> names = [" Метформін ", "Вітамін D", "метформін", string.Empty, "   "];

        // Act
        Result result = diagnosis.AddMedicines(names);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        diagnosis.Medicines.Select(medicine => medicine.Name)
            .Should()
            .Equal("Метформін", "Вітамін D");
    }

    [Fact]
    public void AddMedicines_ShouldReturnFailure_WhenAnyMedicineAlreadyExists()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        diagnosis.AddMedicine("Метформін");
        IReadOnlyCollection<string> names = ["Вітамін D", " метФОрмін "];
        const string expectedMessage = "Ліки з name: метФОрмін вже існують";

        // Act
        Result result = diagnosis.AddMedicines(names);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        diagnosis.Medicines.Should().ContainSingle();
        diagnosis.Medicines.Single().Name.Should().Be("Метформін");
    }

    [Fact]
    public void DeleteMedicineById_ShouldReturnSuccess_WhenMedicineExists()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        diagnosis.AddMedicine("Метформін");

        // Act
        Result result = diagnosis.DeleteMedicine(0);

        // Assert
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
        const int missingId = 999;
        const string expectedMessage = "Ліків з id: 999 не знайдено";

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
        diagnosis.AddMedicine("Метформін");

        // Act
        Result result = diagnosis.DeleteMedicine(" метФОрмін ");

        // Assert
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
        const string expectedMessage = "Ліків з name: Вітамін D не знайдено";

        // Act
        Result result = diagnosis.DeleteMedicine("Вітамін D");

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
        diagnosis.AddMedicines(["Метформін", "Вітамін D"]);

        // Act
        diagnosis.DeleteAllMedicines();

        // Assert
        diagnosis.Medicines.Should().BeEmpty();
    }

    [Fact]
    public void ChangeMedicine_ShouldReturnSuccess_WhenMedicineExistsAndNameIsValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        diagnosis.AddMedicine("Метформін");
        const string newName = "Вітамін D";

        // Act
        Result result = diagnosis.ChangeMedicine(0, newName);

        // Assert
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
        const int missingId = 999;
        const string expectedMessage = "Ліків з id: 999 не знайдено";

        // Act
        Result result = diagnosis.ChangeMedicine(missingId, "Вітамін D");

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
        diagnosis.AddMedicine("Метформін");
        const string currentName = "Метформін";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = diagnosis.ChangeMedicine(0, newName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        diagnosis.Medicines.Single().Name.Should().Be(currentName);
    }

    private static Diagnosis CreateDiagnosis()
    {
        Result<Diagnosis> result = Diagnosis.Create("Інсулінорезистентність", CreateValidBooking());
        return result.Value!;
    }

    private static Booking CreateValidBooking()
    {
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
            "ivan.petrenko@example.com");

        return result.Value!;
    }
}
