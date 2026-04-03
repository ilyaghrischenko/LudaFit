using System.Net;
using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class ServiceTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid_AndDiscountIsNotProvided()
    {
        // Arrange
        const string name = "Консультація";
        const string description = "Первинний прийом";
        const decimal price = 1500m;

        // Act
        Result<Service> result = Service.Create(name, description, price);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Price.Should().Be(price);
        result.Value.Discount.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValid_AndDiscountIsProvided()
    {
        // Arrange
        const string name = "Супровід";
        const string description = "Місячний супровід";
        const decimal price = 3200m;
        Result<Discount> createDiscountResult = Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 10),
            15);
        Discount discount = createDiscountResult.Value!;

        // Act
        Result<Service> result = Service.Create(name, description, price, discount);

        // Assert
        createDiscountResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Discount.Should().BeSameAs(discount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        const string description = "Первинний прийом";
        const decimal price = 1500m;
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Service> result = Service.Create(name!, description, price);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenDescriptionIsNullOrWhiteSpace(string? description)
    {
        // Arrange
        const string name = "Консультація";
        const decimal price = 1500m;
        const string expectedMessage = "Опис послуги не може бути пустим";

        // Act
        Result<Service> result = Service.Create(name, description!, price);

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
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_ShouldReturnFailure_WhenPriceIsZeroOrLess(decimal price)
    {
        // Arrange
        const string name = "Консультація";
        const string description = "Первинний прийом";
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result<Service> result = Service.Create(name, description, price);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizeNameValidation_BeforeDescriptionAndPriceValidation()
    {
        // Arrange
        const string name = "";
        const string description = "";
        const decimal price = 0m;
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result<Service> result = Service.Create(name, description, price);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(2026, 4, 1, 1500)]
    [InlineData(2026, 4, 2, 1200)]
    [InlineData(2026, 4, 5, 1200)]
    [InlineData(2026, 4, 10, 1200)]
    [InlineData(2026, 4, 11, 1500)]
    public void GetFinalPrice_ShouldReturnExpectedPrice_WhenDiscountExists(
        int year,
        int month,
        int day,
        decimal expectedPrice)
    {
        // Arrange
        Result<Discount> createDiscountResult = Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 10),
            20);
        Discount discount = createDiscountResult.Value!;
        Result<Service> createServiceResult = Service.Create(
            "Консультація",
            "Первинний прийом",
            1500m,
            discount);
        Service service = createServiceResult.Value!;
        DateOnly currentDate = new(year, month, day);

        // Act
        decimal result = service.GetFinalPrice(currentDate);

        // Assert
        createDiscountResult.IsSuccess.Should().BeTrue();
        createServiceResult.IsSuccess.Should().BeTrue();
        result.Should().Be(expectedPrice);
    }

    [Fact]
    public void GetFinalPrice_ShouldReturnBasePrice_WhenDiscountIsMissing()
    {
        // Arrange
        const decimal price = 1500m;
        Result<Service> createServiceResult = Service.Create(
            "Консультація",
            "Первинний прийом",
            price);
        Service service = createServiceResult.Value!;

        // Act
        decimal result = service.GetFinalPrice(new DateOnly(2026, 4, 5));

        // Assert
        createServiceResult.IsSuccess.Should().BeTrue();
        result.Should().Be(price);
    }

    [Fact]
    public void GetFinalPrice_ShouldReturnBasePrice_WhenDiscountIsExpired()
    {
        // Arrange
        const decimal price = 1500m;
        Result<Discount> createDiscountResult = Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 10),
            20);
        Discount discount = createDiscountResult.Value!;
        Result expireDiscountResult = discount.Expire(new DateOnly(2026, 4, 11));
        Result<Service> createServiceResult = Service.Create(
            "Консультація",
            "Первинний прийом",
            price,
            discount);
        Service service = createServiceResult.Value!;

        // Act
        decimal result = service.GetFinalPrice(new DateOnly(2026, 4, 5));

        // Assert
        createDiscountResult.IsSuccess.Should().BeTrue();
        expireDiscountResult.IsSuccess.Should().BeTrue();
        createServiceResult.IsSuccess.Should().BeTrue();
        result.Should().Be(price);
    }

    [Fact]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        const string newName = "Новий супровід";

        // Act
        Result result = service.ChangeName(newName);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeName_ShouldReturnFailure_WhenNameIsNullOrWhiteSpace(string? newName)
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        const string originalName = "Консультація";
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result result = service.ChangeName(newName!);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        service.Name.Should().Be(originalName);
    }

    [Fact]
    public void ChangeDescription_ShouldReturnSuccess_WhenDescriptionIsValid()
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        const string newDescription = "Оновлений опис";

        // Act
        Result result = service.ChangeDescription(newDescription);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Description.Should().Be(newDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ChangeDescription_ShouldReturnFailure_WhenDescriptionIsNullOrWhiteSpace(string? newDescription)
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        const string originalDescription = "Первинний прийом";
        const string expectedMessage = "Опис послуги не може бути пустим";

        // Act
        Result result = service.ChangeDescription(newDescription!);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        service.Description.Should().Be(originalDescription);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(500)]
    public void ChangePrice_ShouldReturnSuccess_WhenPriceIsGreaterThanZero(decimal newPrice)
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;

        // Act
        Result result = service.ChangePrice(newPrice);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Price.Should().Be(newPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void ChangePrice_ShouldReturnFailure_WhenPriceIsZeroOrLess(decimal newPrice)
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        const decimal originalPrice = 1500m;
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result result = service.ChangePrice(newPrice);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        service.Price.Should().Be(originalPrice);
    }

    [Fact]
    public void AddDiscount_ShouldReturnSuccess_WhenArgumentsAreValid()
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(2026, 4, 2);
        DateOnly endDate = new(2026, 4, 10);
        const uint percent = 25;

        // Act
        Result result = service.AddDiscount(currentDate, startDate, endDate, percent);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Discount.Should().NotBeNull();
        service.Discount!.DateRange.Start.Should().Be(startDate);
        service.Discount.DateRange.End.Should().Be(endDate);
        service.Discount.Percent.Should().Be(percent);
    }

    [Theory]
    [InlineData(2026, 4, 1, 2026, 4, 10, 25, "Дата початку знижки не може бути раніше ніж сьогодні")]
    [InlineData(2026, 4, 3, 2026, 4, 2, 25, "Дата початку знижки не може бути пізніше ніж дата її кінця")]
    [InlineData(2026, 4, 2, 2026, 4, 10, 0, "Відсоток знижки не може бути 0 або більше 100")]
    [InlineData(2026, 4, 2, 2026, 4, 10, 101, "Відсоток знижки не може бути 0 або більше 100")]
    public void AddDiscount_ShouldReturnFailure_WhenArgumentsAreInvalid(
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay,
        uint percent,
        string expectedMessage)
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 2);
        DateOnly startDate = new(startYear, startMonth, startDay);
        DateOnly endDate = new(endYear, endMonth, endDay);

        // Act
        Result result = service.AddDiscount(currentDate, startDate, endDate, percent);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        service.Discount.Should().BeNull();
    }

    [Fact]
    public void AddDiscount_ShouldReplaceExistingDiscount_WhenArgumentsAreValid()
    {
        // Arrange
        Result<Discount> createDiscountResult = Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 8),
            10);
        Discount existingDiscount = createDiscountResult.Value!;
        Result<Service> createServiceResult = Service.Create(
            "Консультація",
            "Первинний прийом",
            1500m,
            existingDiscount);
        Service service = createServiceResult.Value!;
        DateOnly newStartDate = new(2026, 4, 3);
        DateOnly newEndDate = new(2026, 4, 12);
        const uint newPercent = 30;

        // Act
        Result result = service.AddDiscount(new DateOnly(2026, 4, 2), newStartDate, newEndDate, newPercent);

        // Assert
        createDiscountResult.IsSuccess.Should().BeTrue();
        createServiceResult.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        service.Discount.Should().NotBeNull();
        service.Discount.Should().NotBeSameAs(existingDiscount);
        service.Discount!.DateRange.Start.Should().Be(newStartDate);
        service.Discount.DateRange.End.Should().Be(newEndDate);
        service.Discount.Percent.Should().Be(newPercent);
    }

    [Fact]
    public void RemoveDiscount_ShouldClearDiscount_WhenDiscountExists()
    {
        // Arrange
        Result<Discount> createDiscountResult = Discount.Create(
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 2),
            new DateOnly(2026, 4, 10),
            20);
        Discount discount = createDiscountResult.Value!;
        Result<Service> createServiceResult = Service.Create(
            "Консультація",
            "Первинний прийом",
            1500m,
            discount);
        Service service = createServiceResult.Value!;

        // Act
        service.RemoveDiscount();

        // Assert
        createDiscountResult.IsSuccess.Should().BeTrue();
        createServiceResult.IsSuccess.Should().BeTrue();
        service.Discount.Should().BeNull();
    }

    [Fact]
    public void RemoveDiscount_ShouldKeepDiscountNull_WhenDiscountIsMissing()
    {
        // Arrange
        Result<Service> createResult = CreateValidService();
        Service service = createResult.Value!;

        // Act
        service.RemoveDiscount();

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        service.Discount.Should().BeNull();
    }

    private static Result<Service> CreateValidService()
    {
        return Service.Create("Консультація", "Первинний прийом", 1500m);
    }
}
