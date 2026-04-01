using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.Enums;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class DiscountTests
{
    [Theory]
    [InlineData(0, 0, 1u)]
    [InlineData(0, 5, 100u)]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid(int startOffsetDays, int endOffsetDays, uint percent)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(startOffsetDays);
        DateOnly endDate = currentDate.AddDays(endOffsetDays);

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.StartDate.Should().Be(startDate);
        result.Value.EndDate.Should().Be(endDate);
        result.Value.Percent.Should().Be(percent);
        result.Value.Status.Should().Be(DiscountStatus.Active);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenStartDateIsEarlierThanCurrentDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(-1);
        DateOnly endDate = currentDate.AddDays(5);
        const uint percent = 10;
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEndDateIsEarlierThanCurrentDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate;
        DateOnly endDate = currentDate.AddDays(-1);
        const uint percent = 10;
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenStartDateIsLaterThanEndDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(5);
        DateOnly endDate = currentDate.AddDays(4);
        const uint percent = 10;
        const string expectedMessage = "Дата початку знижки не може бути пізніше ніж дата її кінця";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(101u)]
    public void Create_ShouldReturnFailure_WhenPercentIsOutOfRange(uint percent)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate;
        DateOnly endDate = currentDate.AddDays(5);
        const string expectedMessage = "Відсоток знижки не може бути 0 або більше 100";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldPrioritizeStartDateValidation_BeforeOtherValidationErrors()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(-1);
        DateOnly endDate = currentDate.AddDays(-2);
        const uint percent = 0;
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(-1, false)]
    [InlineData(6, false)]
    public void IsActive_ShouldReturnExpectedValue_ForDateWithinOrOutsideRange(int currentOffsetDays, bool expectedResult)
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly checkedDate = currentDate.AddDays(currentOffsetDays);

        // Act
        bool result = discount.IsActive(checkedDate);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenDiscountIsExpired()
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly currentDate = new(2026, 4, 7);
        Result expireResult = discount.Expire(currentDate);

        // Act
        bool result = discount.IsActive(currentDate);

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ChangeEndDate_ShouldReturnSuccess_WhenNewEndDateIsValid(int newEndOffsetDays)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly newEndDate = currentDate.AddDays(newEndOffsetDays);

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.EndDate.Should().Be(newEndDate);
        discount.Status.Should().Be(DiscountStatus.Active);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenDiscountIsExpired()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result expireResult = discount.Expire(currentDate.AddDays(2));
        DateOnly originalEndDate = discount.EndDate;
        DateOnly newEndDate = currentDate.AddDays(10);
        const string expectedMessage = "Неможливо змінити дату кінця знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangeEndDate(currentDate.AddDays(2), newEndDate);

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.EndDate.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenNewEndDateIsEarlierThanCurrentDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly originalEndDate = discount.EndDate;
        DateOnly newEndDate = currentDate.AddDays(-1);
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.EndDate.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenNewEndDateIsEarlierThanStartDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 2, endOffsetDays: 5, percent: 20);
        DateOnly originalEndDate = discount.EndDate;
        DateOnly newEndDate = currentDate.AddDays(1);
        const string expectedMessage = "Дата початку знижки не може бути пізніше ніж дата її кінця";

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.EndDate.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldPrioritizeExpiredValidation_BeforeDateValidation()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result expireResult = discount.Expire(currentDate.AddDays(2));
        DateOnly originalEndDate = discount.EndDate;
        DateOnly invalidEndDate = currentDate.AddDays(-1);
        const string expectedMessage = "Неможливо змінити дату кінця знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangeEndDate(currentDate.AddDays(2), invalidEndDate);

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.EndDate.Should().Be(originalEndDate);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(100u)]
    public void ChangePercent_ShouldReturnSuccess_WhenPercentIsWithinRange(uint newPercent)
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);

        // Act
        Result result = discount.ChangePercent(newPercent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Percent.Should().Be(newPercent);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(101u)]
    public void ChangePercent_ShouldReturnFailure_WhenPercentIsOutOfRange(uint newPercent)
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        const uint originalPercent = 20;
        const string expectedMessage = "Відсоток знижки не може бути 0 або більше 100";

        // Act
        Result result = discount.ChangePercent(newPercent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.Percent.Should().Be(originalPercent);
    }

    [Fact]
    public void ChangePercent_ShouldReturnFailure_WhenDiscountIsExpired()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result expireResult = discount.Expire(currentDate.AddDays(2));
        const uint originalPercent = 20;
        const uint newPercent = 30;
        const string expectedMessage = "Неможливо змінити відсоток знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangePercent(newPercent);

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.Percent.Should().Be(originalPercent);
    }

    [Fact]
    public void ChangePercent_ShouldPrioritizeExpiredValidation_BeforePercentValidation()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result expireResult = discount.Expire(currentDate.AddDays(2));
        const uint originalPercent = 20;
        const uint invalidPercent = 0;
        const string expectedMessage = "Неможливо змінити відсоток знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangePercent(invalidPercent);

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.Percent.Should().Be(originalPercent);
    }

    [Fact]
    public void Expire_ShouldReturnSuccess_AndChangeStatus_WhenCurrentDateIsAfterEndDate()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly expireDate = currentDate.AddDays(6);

        // Act
        Result result = discount.Expire(expireDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Status.Should().Be(DiscountStatus.Expired);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void Expire_ShouldReturnFailure_WhenDiscountHasNotEndedYet(int currentOffsetDays)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        DateOnly expireDate = currentDate.AddDays(currentOffsetDays);
        const string expectedMessage = "Знижка ще активна, її не можливо позначити як закінчену";

        // Act
        Result result = discount.Expire(expireDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        discount.Status.Should().Be(DiscountStatus.Active);
    }

    [Fact]
    public void Expire_ShouldReturnSuccess_WhenDiscountIsAlreadyExpired()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result firstExpireResult = discount.Expire(currentDate.AddDays(2));

        // Act
        Result secondExpireResult = discount.Expire(currentDate.AddDays(3));

        // Assert
        firstExpireResult.IsSuccess.Should().BeTrue();
        secondExpireResult.IsSuccess.Should().BeTrue();
        secondExpireResult.IsFailure.Should().BeFalse();
        secondExpireResult.ErrorDetails.Should().BeNull();
        discount.Status.Should().Be(DiscountStatus.Expired);
    }

    private static Discount CreateDiscount(int startOffsetDays, int endOffsetDays, uint percent)
    {
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(startOffsetDays);
        DateOnly endDate = currentDate.AddDays(endOffsetDays);
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        return result.Value!;
    }
}
