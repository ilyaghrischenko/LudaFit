using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class SpecialistTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid()
    {
        // Arrange
        const string name = "Іван";
        const string photoUrl = "https://cdn.example.com/photo.jpg";
        const string description = "Досвідчений спеціаліст";
        const int startWorkHour = 9;
        const int startWorkMinute = 15;
        const int endWorkHour = 18;
        const int endWorkMinute = 45;

        // Act
        Result<Specialist> result = Specialist.Create(
            name,
            photoUrl,
            description,
            startWorkHour,
            startWorkMinute,
            endWorkHour,
            endWorkMinute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.PhotoUrl.Should().Be(photoUrl);
        result.Value.Description.Should().Be(description);
        result.Value.WorkTime.Should().Be(TimeRange.Create(
            startWorkHour,
            startWorkMinute,
            endWorkHour,
            endWorkMinute).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        const string photoUrl = "https://cdn.example.com/photo.jpg";
        const string description = "Досвідчений спеціаліст";
        const string expectedMessage = "Ім'я не може бути пустим";

        // Act
        Result<Specialist> result = Specialist.Create(name!, photoUrl, description, 9, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenPhotoUrlIsNullOrWhiteSpace(string? photoUrl)
    {
        // Arrange
        const string name = "Іван";
        const string description = "Досвідчений спеціаліст";
        const string expectedMessage = "Фотографія не може бути пустою";

        // Act
        Result<Specialist> result = Specialist.Create(name, photoUrl!, description, 9, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenDescriptionIsNullOrWhiteSpace(string? description)
    {
        // Arrange
        const string name = "Іван";
        const string photoUrl = "https://cdn.example.com/photo.jpg";
        const string expectedMessage = "Опис не може бути пустим";

        // Act
        Result<Specialist> result = Specialist.Create(name, photoUrl, description!, 9, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenWorkTimeIsInvalid()
    {
        // Arrange
        const string name = "Іван";
        const string photoUrl = "https://cdn.example.com/photo.jpg";
        const string description = "Досвідчений спеціаліст";
        const string expectedMessage = "Година початку або кінця не може бути від'ємною";

        // Act
        Result<Specialist> result = Specialist.Create(name, photoUrl, description, -1, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldPrioritizeNameValidation_BeforePhotoUrlDescriptionAndWorkTimeValidation()
    {
        // Arrange
        const string name = "";
        const string photoUrl = "";
        const string description = "";
        const string expectedMessage = "Ім'я не може бути пустим";

        // Act
        Result<Specialist> result = Specialist.Create(name, photoUrl, description, -1, 70, 25, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string newName = "Петро";

        // Act
        Result result = specialist.ChangeName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        specialist.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string currentName = "Іван";
        const string expectedMessage = "Ім'я не може бути пустим";

        // Act
        Result result = specialist.ChangeName(newName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.Name.Should().Be(currentName);
    }

    [Fact]
    public void ChangePhotoUrl_ShouldReturnSuccess_WhenPhotoUrlIsValidAndDifferent()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string newPhotoUrl = "https://cdn.example.com/new-photo.jpg";

        // Act
        Result result = specialist.ChangePhotoUrl(newPhotoUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        specialist.PhotoUrl.Should().Be(newPhotoUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangePhotoUrl_ShouldReturnFailure_WhenPhotoUrlIsNullOrWhiteSpace(string? newPhotoUrl)
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string currentPhotoUrl = "https://cdn.example.com/photo.jpg";
        const string expectedMessage = "Нова фотографія не може бути пустою";

        // Act
        Result result = specialist.ChangePhotoUrl(newPhotoUrl!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.PhotoUrl.Should().Be(currentPhotoUrl);
    }

    [Fact]
    public void ChangePhotoUrl_ShouldReturnFailure_WhenPhotoUrlMatchesCurrentPhotoUrl()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string currentPhotoUrl = "https://cdn.example.com/photo.jpg";
        const string expectedMessage = "Нова фотографія повинна відрізнятися";

        // Act
        Result result = specialist.ChangePhotoUrl(currentPhotoUrl);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.PhotoUrl.Should().Be(currentPhotoUrl);
    }

    [Fact]
    public void ChangeDescription_ShouldReturnSuccess_WhenDescriptionIsValidAndDifferent()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string newDescription = "Новий опис спеціаліста";

        // Act
        Result result = specialist.ChangeDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        specialist.Description.Should().Be(newDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeDescription_ShouldReturnFailure_WhenDescriptionIsNullOrWhiteSpace(string? description)
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string currentDescription = "Досвідчений спеціаліст";
        const string expectedMessage = "Опис не може бути пустим";

        // Act
        Result result = specialist.ChangeDescription(description!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.Description.Should().Be(currentDescription);
    }

    [Fact]
    public void ChangeDescription_ShouldReturnFailure_WhenDescriptionMatchesCurrentDescription()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        const string currentDescription = "Досвідчений спеціаліст";
        const string expectedMessage = "Новий опис повинен відрізнятися";

        // Act
        Result result = specialist.ChangeDescription(currentDescription);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.Description.Should().Be(currentDescription);
    }

    [Fact]
    public void ChangeWorkTime_ShouldReturnSuccess_WhenWorkTimeIsValidAndDifferent()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        TimeRange originalWorkTime = specialist.WorkTime;
        TimeRange expectedWorkTime = TimeRange.Create(8, 30, 17, 15).Value!;

        // Act
        Result result = specialist.ChangeWorkTime(8, 30, 17, 15);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        specialist.WorkTime.Should().Be(expectedWorkTime);
        specialist.WorkTime.Should().NotBe(originalWorkTime);
    }

    [Fact]
    public void ChangeWorkTime_ShouldReturnFailure_WhenWorkTimeIsInvalid()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        TimeRange currentWorkTime = specialist.WorkTime;
        const string expectedMessage = "Година початку або кінця не може бути від'ємною";

        // Act
        Result result = specialist.ChangeWorkTime(-1, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.WorkTime.Should().Be(currentWorkTime);
    }

    [Fact]
    public void ChangeWorkTime_ShouldReturnFailure_WhenWorkTimeMatchesCurrentWorkTime()
    {
        // Arrange
        Specialist specialist = CreateSpecialist();
        TimeRange currentWorkTime = specialist.WorkTime;
        const string expectedMessage = "Новий графік роботи повністю співпадає з поточним";

        // Act
        Result result = specialist.ChangeWorkTime(9, 0, 18, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        specialist.WorkTime.Should().Be(currentWorkTime);
    }

    private static Specialist CreateSpecialist()
    {
        Result<Specialist> result = Specialist.Create(
            "Іван",
            "https://cdn.example.com/photo.jpg",
            "Досвідчений спеціаліст",
            9,
            0,
            18,
            0);

        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
