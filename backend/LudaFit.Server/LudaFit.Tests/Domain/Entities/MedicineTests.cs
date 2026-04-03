using System.Net;
using System.Reflection;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class MedicineTests
{
    [Theory]
    [InlineData("Парацетамол")]
    [InlineData("  Ibuprofen  ")]
    public void Create_ShouldReturnSuccess_WhenNameAndDiagnosisAreValid(string name)
    {
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз", 17);

        // Act
        Result<Medicine> result = Medicine.Create(name, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Diagnosis.Should().BeSameAs(diagnosis);
        result.Value.DiagnosisId.Should().Be(17);
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
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз", 17);
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
        // Arrange
        Diagnosis diagnosis = CreateDiagnosis("Гіпотиреоз", 17);

        // Act
        Result<Medicine> result = Medicine.Create(name, diagnosis);

        // Assert
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static Diagnosis CreateDiagnosis(string name, int id)
    {
        // Arrange
        Booking booking = CreateBooking();

        // Act
        Result<Diagnosis> result = Diagnosis.Create(name, booking);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Diagnosis diagnosis = result.Value!;
        SetEntityId(diagnosis, id);
        return diagnosis;
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
