using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.Domain.Enums;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class DiscountTests
{
    [Theory]
    [InlineData(2026, 4, 2, 2026, 4, 2, 1)]
    [InlineData(2026, 4, 2, 2026, 4, 10, 25)]
    [InlineData(2026, 4, 10, 2026, 4, 10, 100)]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay,
        uint percent)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(startYear, startMonth, startDay);
        DateOnly endDate = new(endYear, endMonth, endDay);

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.DateRange.Start.Should().Be(startDate);
        result.Value.DateRange.End.Should().Be(endDate);
        result.Value.Percent.Should().Be(percent);
        result.Value.Status.Should().Be(DiscountStatus.Active);
        result.Value.Services.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2026, 4, 1, 2026, 4, 2, "Дата початку знижки не може бути раніше ніж сьогодні")]
    [InlineData(2026, 4, 2, 2026, 4, 1, "Дата кінця знижки не може бути раніше ніж сьогодні")]
    [InlineData(2026, 4, 3, 2026, 4, 2, "Дата початку знижки не може бути пізніше ніж дата її кінця")]
    public void Create_ShouldReturnFailure_WhenDateRangeIsInvalid(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay,
        string expectedMessage)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(startYear, startMonth, startDay);
        DateOnly endDate = new(endYear, endMonth, endDay);

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, 10);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Create_ShouldReturnFailure_WhenPercentIsOutOfRange(uint percent)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(2026, 4, 2);
        DateOnly endDate = new(2026, 4, 10);
        const string expectedMessage = "Відсоток знижки не може бути 0 або більше 100";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeDateRangeValidation_BeforePercentValidation()
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(2026, 4, 1);
        DateOnly endDate = new(2026, 4, 10);
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, 0);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(2026, 4, 2, true)]
    [InlineData(2026, 4, 5, true)]
    [InlineData(2026, 4, 10, true)]
    [InlineData(2026, 4, 1, false)]
    [InlineData(2026, 4, 11, false)]
    public void IsActive_ShouldReturnExpectedValue_WhenDiscountStatusIsActive(
        int year,
        int month,
        int day,
        bool expectedResult)
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        DateOnly currentDate = new(year, month, day);

        // Act
        bool result = discount.IsActive(currentDate);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenDiscountIsExpired()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result expireResult = discount.Expire(new DateOnly(2026, 4, 11));

        // Act
        bool result = discount.IsActive(new DateOnly(2026, 4, 5));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        expireResult.IsSuccess.Should().BeTrue();
        result.Should().BeFalse();
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnSuccess_WhenNewEndDateIsValid()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly newEndDate = new(2026, 4, 15);

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.DateRange.Start.Should().Be(new DateOnly(2026, 4, 2));
        discount.DateRange.End.Should().Be(newEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenNewEndDateIsEarlierThanCurrentDate()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly originalEndDate = discount.DateRange.End;
        DateOnly newEndDate = new(2026, 3, 31);
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.DateRange.End.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenCurrentDateIsLaterThanStartDate()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 5);
        DateOnly originalEndDate = discount.DateRange.End;
        DateOnly newEndDate = new(2026, 4, 12);
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.DateRange.End.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenNewEndDateIsEarlierThanStartDate()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly originalEndDate = discount.DateRange.End;
        DateOnly newEndDate = new(2026, 4, 1);
        const string expectedMessage = "Дата кінця знижки не може бути раніше ніж сьогодні";

        // Act
        Result result = discount.ChangeEndDate(currentDate, newEndDate);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.DateRange.End.Should().Be(originalEndDate);
    }

    [Fact]
    public void ChangeEndDate_ShouldReturnFailure_WhenDiscountIsExpired()
    {
        // Arrange
        Result<Discount> createResult = CreateExpiredDiscount();
        Discount discount = createResult.Value!;
        DateOnly originalEndDate = discount.DateRange.End;
        const string expectedMessage = "Неможливо змінити дату кінця знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangeEndDate(new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 20));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.DateRange.End.Should().Be(originalEndDate);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ChangePercent_ShouldReturnSuccess_WhenPercentIsInRange(uint newPercent)
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;

        // Act
        Result result = discount.ChangePercent(newPercent);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Percent.Should().Be(newPercent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void ChangePercent_ShouldReturnFailure_WhenPercentIsOutOfRange(uint newPercent)
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        const uint originalPercent = 20;
        const string expectedMessage = "Відсоток знижки не може бути 0 або більше 100";

        // Act
        Result result = discount.ChangePercent(newPercent);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.Percent.Should().Be(originalPercent);
    }

    [Fact]
    public void ChangePercent_ShouldReturnFailure_WhenDiscountIsExpired()
    {
        // Arrange
        Result<Discount> createResult = CreateExpiredDiscount();
        Discount discount = createResult.Value!;
        const uint originalPercent = 20;
        const string expectedMessage = "Неможливо змінити відсоток знижки, вона вже скінчилась";

        // Act
        Result result = discount.ChangePercent(30);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.Percent.Should().Be(originalPercent);
    }

    [Fact]
    public void Expire_ShouldReturnSuccess_AndSetExpiredStatus_WhenCurrentDateIsAfterEndDate()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;

        // Act
        Result result = discount.Expire(new DateOnly(2026, 4, 11));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Status.Should().Be(DiscountStatus.Expired);
    }

    [Theory]
    [InlineData(2026, 4, 2)]
    [InlineData(2026, 4, 10)]
    public void Expire_ShouldReturnFailure_WhenDiscountIsStillActive(
        int year,
        int month,
        int day)
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        const string expectedMessage = "Знижка ще активна, її не можливо позначити як закінчену";

        // Act
        Result result = discount.Expire(new DateOnly(year, month, day));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        discount.Status.Should().Be(DiscountStatus.Active);
    }

    [Fact]
    public void Expire_ShouldReturnSuccess_WhenDiscountIsAlreadyExpired()
    {
        // Arrange
        Result<Discount> createResult = CreateExpiredDiscount();
        Discount discount = createResult.Value!;

        // Act
        Result result = discount.Expire(new DateOnly(2026, 4, 20));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Status.Should().Be(DiscountStatus.Expired);
    }

    [Fact]
    public void AddService_ShouldReturnSuccess_WhenServiceNameIsUnique()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result<Service> createServiceResult = CreateService("Консультація");
        Service service = createServiceResult.Value!;

        // Act
        Result result = discount.AddService(service);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        createServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Services.Should().ContainSingle().Which.Should().BeSameAs(service);
    }

    [Fact]
    public void AddService_ShouldReturnFailure_WhenServiceWithSameNameAlreadyExists_IgnoringCaseAndWhitespace()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result<Service> firstServiceResult = CreateService(" Консультація ");
        Result<Service> secondServiceResult = CreateService("консультація");
        Service firstService = firstServiceResult.Value!;
        Service secondService = secondServiceResult.Value!;
        Result addFirstServiceResult = discount.AddService(firstService);
        const string expectedMessage = "Сервіс з назвою консультація вже існує";

        // Act
        Result result = discount.AddService(secondService);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        firstServiceResult.IsSuccess.Should().BeTrue();
        secondServiceResult.IsSuccess.Should().BeTrue();
        addFirstServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        discount.Services.Should().ContainSingle().Which.Should().BeSameAs(firstService);
    }

    [Fact]
    public void AddServices_ShouldReturnSuccess_WhenAllServiceNamesAreUnique()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result<Service> firstServiceResult = CreateService("Консультація");
        Result<Service> secondServiceResult = CreateService("Супровід");
        Service[] services =
        [
            firstServiceResult.Value!,
            secondServiceResult.Value!
        ];

        // Act
        Result result = discount.AddServices(services);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        firstServiceResult.IsSuccess.Should().BeTrue();
        secondServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        discount.Services.Should().HaveCount(2);
        discount.Services.Should().Contain(services[0]);
        discount.Services.Should().Contain(services[1]);
    }

    [Fact]
    public void AddServices_ShouldReturnFailure_AndKeepAlreadyAddedServices_WhenDuplicateAppearsInBatch()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result<Service> firstServiceResult = CreateService("Консультація");
        Result<Service> duplicateServiceResult = CreateService(" консультація ");
        Service firstService = firstServiceResult.Value!;
        Service duplicateService = duplicateServiceResult.Value!;
        Service[] services =
        [
            firstService,
            duplicateService
        ];
        const string expectedMessage = "Сервіс з назвою  консультація  вже існує";

        // Act
        Result result = discount.AddServices(services);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        firstServiceResult.IsSuccess.Should().BeTrue();
        duplicateServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        discount.Services.Should().ContainSingle().Which.Should().BeSameAs(firstService);
    }

    [Fact]
    public void AddServices_ShouldReturnFailure_WhenDiscountAlreadyContainsServiceWithSameName()
    {
        // Arrange
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result<Service> existingServiceResult = CreateService("Консультація");
        Result<Service> newServiceResult = CreateService(" консультація ");
        Service existingService = existingServiceResult.Value!;
        Service newService = newServiceResult.Value!;
        Result addExistingServiceResult = discount.AddService(existingService);
        Service[] services = [newService];
        const string expectedMessage = "Сервіс з назвою  консультація  вже існує";

        // Act
        Result result = discount.AddServices(services);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        existingServiceResult.IsSuccess.Should().BeTrue();
        newServiceResult.IsSuccess.Should().BeTrue();
        addExistingServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.Conflict);
        discount.Services.Should().ContainSingle().Which.Should().BeSameAs(existingService);
    }

    private static Result<Discount> CreateValidDiscount()
        => Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 10),
            20);

    private static Result<Discount> CreateExpiredDiscount()
    {
        Result<Discount> createResult = CreateValidDiscount();
        Discount discount = createResult.Value!;
        Result expireResult = discount.Expire(new DateOnly(2026, 4, 11));

        createResult.IsSuccess.Should().BeTrue();
        expireResult.IsSuccess.Should().BeTrue();

        return createResult;
    }

    private static Result<Service> CreateService(string name)
        => Service.Create(name, "Опис сервісу", 1000m);
}
