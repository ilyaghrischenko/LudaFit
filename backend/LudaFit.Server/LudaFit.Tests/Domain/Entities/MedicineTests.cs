using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class MedicineTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string name = "Метформін";

        // Act
        Result<Medicine> result = Medicine.Create(name, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Diagnosis.Should().BeSameAs(diagnosis);
        result.Value.DiagnosisId.Should().Be(diagnosis.Id);
    }

    [Fact]
    public void Create_ShouldPreserveProvidedName_WhenItContainsOuterWhitespace()
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string name = " Метформін ";

        // Act
        Result<Medicine> result = Medicine.Create(name, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis();
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result<Medicine> result = Medicine.Create(name!, diagnosis);

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
        Result<Medicine> createResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createResult.Value!;
        const string newName = "Вітамін D";

        // Act
        Result result = medicine.ChangeName(newName);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        medicine.Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeName_ShouldPreserveProvidedName_WhenItContainsOuterWhitespace()
    {
        // Arrange
        Result<Medicine> createResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createResult.Value!;
        const string newName = " Вітамін D ";

        // Act
        Result result = medicine.ChangeName(newName);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
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
        Result<Medicine> createResult = Medicine.Create("Метформін", CreateDiagnosis());
        Medicine medicine = createResult.Value!;
        const string currentName = "Метформін";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = medicine.ChangeName(newName!);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        medicine.Name.Should().Be(currentName);
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
