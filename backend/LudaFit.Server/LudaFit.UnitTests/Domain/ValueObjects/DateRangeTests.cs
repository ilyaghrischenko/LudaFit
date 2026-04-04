using System.Net;
using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.ValueObjects;

public sealed class DateRangeTests
{
    [Theory]
    [InlineData(2026, 4, 2, 2026, 4, 2)]
    [InlineData(2026, 4, 2, 2026, 4, 3)]
    [InlineData(2026, 4, 10, 2026, 4, 10)]
    [InlineData(2026, 4, 10, 2026, 4, 30)]
    public void Create_ShouldReturnSuccess_WhenDateRangeIsValid(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(startYear, startMonth, startDay);
        DateOnly end = new(endYear, endMonth, endDay);

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Start.Should().Be(start);
        result.Value.End.Should().Be(end);
    }

    [Theory]
    [InlineData(2026, 4, 1, 2026, 4, 2)]
    [InlineData(2025, 12, 31, 2026, 4, 2)]
    public void Create_ShouldReturnFailure_WhenStartDateIsEarlierThanCurrentDate(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(startYear, startMonth, startDay);
        DateOnly end = new(endYear, endMonth, endDay);
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(2026, 4, 2, 2026, 4, 1)]
    [InlineData(2026, 4, 3, 2025, 12, 31)]
    public void Create_ShouldReturnFailure_WhenEndDateIsEarlierThanCurrentDate(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(startYear, startMonth, startDay);
        DateOnly end = new(endYear, endMonth, endDay);
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(2026, 4, 3, 2026, 4, 2)]
    [InlineData(2026, 5, 1, 2026, 4, 30)]
    public void Create_ShouldReturnFailure_WhenStartDateIsLaterThanEndDate(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(startYear, startMonth, startDay);
        DateOnly end = new(endYear, endMonth, endDay);
        const string expectedMessage = "Дата початку знижки не може бути пізніше ніж дата її кінця";

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeStartDateValidation_BeforeEndDateValidation()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(2026, 4, 1);
        DateOnly end = new(2026, 4, 1);
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeEndDateValidation_BeforeStartLaterThanEndValidation()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly start = new(2026, 4, 3);
        DateOnly end = new(2026, 4, 1);
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result<DateRange> result = DateRange.Create(currentDate, start, end);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
