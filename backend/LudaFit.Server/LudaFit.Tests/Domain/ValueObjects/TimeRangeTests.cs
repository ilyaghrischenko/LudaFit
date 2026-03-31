using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.ValueObjects;

public sealed class TimeRangeTests
{
    [Theory]
    [InlineData(0, 0, 0, 1)]
    [InlineData(9, 15, 10, 45)]
    [InlineData(23, 58, 23, 59)]
    [InlineData(0, 0, 23, 59)]
    public void Create_ShouldReturnSuccess_WhenTimeRangeIsValid(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Start.Should().Be(new TimeOnly(startHour, startMinute));
        result.Value.End.Should().Be(new TimeOnly(endHour, endMinute));
    }

    [Theory]
    [InlineData(-1, 0, 10, 0)]
    [InlineData(10, 0, -1, 0)]
    [InlineData(-1, 0, -1, 0)]
    public void Create_ShouldReturnFailure_WhenHourIsNegative(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange
        const string expectedMessage = "Година початку або кінця не може бути від'ємною";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(24, 0, 10, 0)]
    [InlineData(10, 0, 24, 0)]
    [InlineData(24, 0, 24, 0)]
    public void Create_ShouldReturnFailure_WhenHourExceeds23(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange
        const string expectedMessage = "Година початку або кінця не може бути більше 23";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(10, -1, 11, 0)]
    [InlineData(10, 0, 11, -1)]
    [InlineData(10, -1, 11, -1)]
    public void Create_ShouldReturnFailure_WhenMinuteIsNegative(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange
        const string expectedMessage = "Хвилина початку або кінця не може бути від'ємною";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(10, 60, 11, 0)]
    [InlineData(10, 0, 11, 60)]
    [InlineData(10, 60, 11, 60)]
    public void Create_ShouldReturnFailure_WhenMinuteExceeds59(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange
        const string expectedMessage = "Хвилина початку або кінця не може бути більше 59";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenStartHourIsGreaterThanEndHour()
    {
        // Arrange
        const int startHour = 18;
        const int startMinute = 0;
        const int endHour = 17;
        const int endMinute = 59;
        const string expectedMessage = "Година початку не може бути більшою за годину кінця";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(10, 30, 10, 30)]
    [InlineData(10, 31, 10, 30)]
    [InlineData(0, 0, 0, 0)]
    public void Create_ShouldReturnFailure_WhenHoursAreEqualAndStartMinuteIsNotLessThanEndMinute(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        // Arrange
        const string expectedMessage = "Якщо години початку і кінця рівні, то хвилина початку"
                                       + " не може бути більшою за хвилину кінця або дорівнюват їй";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldPrioritizeNegativeHourValidation_BeforeOtherValidationErrors()
    {
        // Arrange
        const int startHour = -1;
        const int startMinute = 61;
        const int endHour = 0;
        const int endMinute = 0;
        const string expectedMessage = "Година початку або кінця не може бути від'ємною";

        // Act
        Result<TimeRange> result = TimeRange.Create(startHour, startMinute, endHour, endMinute);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }
}
