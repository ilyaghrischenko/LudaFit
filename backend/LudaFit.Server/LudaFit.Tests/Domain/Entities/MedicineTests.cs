using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class MedicineTests
{
    [Theory]
    [InlineData("Парацетамол")]
    [InlineData("  Ibuprofen  ")]
    public void Create_ShouldReturnSuccess_WhenNameIsValid(string name)
    {
        // Arrange

        // Act
        Result<Medicine> result = Medicine.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
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
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result<Medicine> result = Medicine.Create(name!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Нурофен")]
    [InlineData("  Aspirin  ")]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid(string newName)
    {
        // Arrange
        Medicine medicine = CreateMedicine("Парацетамол");

        // Act
        Result result = medicine.ChangeName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        medicine.Name.Should().Be(newName);
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
        Medicine medicine = CreateMedicine("Парацетамол");
        const string currentName = "Парацетамол";
        const string expectedMessage = "Назва ліків не може бути пустою";

        // Act
        Result result = medicine.ChangeName(newName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        medicine.Name.Should().Be(currentName);
    }

    private static Medicine CreateMedicine(string name)
    {
        Result<Medicine> result = Medicine.Create(name);
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
