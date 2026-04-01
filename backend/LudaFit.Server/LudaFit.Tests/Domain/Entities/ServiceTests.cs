using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Tests.Domain.Entities;

public sealed class ServiceTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValidWithoutDiscount()
    {
        // Arrange
        const string name = "Консультація";
        const string description = "Первинна консультація";
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
    public void Create_ShouldReturnSuccess_WhenArgumentsAreValidWithDiscount()
    {
        // Arrange
        const string name = "Консультація";
        const string description = "Первинна консультація";
        const decimal price = 1500m;
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);

        // Act
        Result<Service> result = Service.Create(name, description, price, discount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
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
        const string description = "Первинна консультація";
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
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_ShouldReturnFailure_WhenPriceIsLessThanOrEqualToZero(decimal price)
    {
        // Arrange
        const string name = "Консультація";
        const string description = "Первинна консультація";
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result<Service> result = Service.Create(name, description, price);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
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

    [Fact]
    public void ChangeName_ShouldReturnSuccess_WhenNameIsValid()
    {
        // Arrange
        Service service = CreateService();
        const string newName = "Повторна консультація";

        // Act
        Result result = service.ChangeName(newName);

        // Assert
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
        Service service = CreateService();
        const string currentName = "Консультація";
        const string expectedMessage = "Назва послуги не може бути пустою";

        // Act
        Result result = service.ChangeName(newName!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        service.Name.Should().Be(currentName);
    }

    [Fact]
    public void ChangeDescription_ShouldReturnSuccess_WhenDescriptionIsValid()
    {
        // Arrange
        Service service = CreateService();
        const string newDescription = "Оновлений опис послуги";

        // Act
        Result result = service.ChangeDescription(newDescription);

        // Assert
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
        Service service = CreateService();
        const string currentDescription = "Первинна консультація";
        const string expectedMessage = "Опис послуги не може бути пустим";

        // Act
        Result result = service.ChangeDescription(newDescription!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        service.Description.Should().Be(currentDescription);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2500.50)]
    public void ChangePrice_ShouldReturnSuccess_WhenPriceIsGreaterThanZero(decimal newPrice)
    {
        // Arrange
        Service service = CreateService();

        // Act
        Result result = service.ChangePrice(newPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Price.Should().Be(newPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void ChangePrice_ShouldReturnFailure_WhenPriceIsLessThanOrEqualToZero(decimal newPrice)
    {
        // Arrange
        Service service = CreateService();
        const decimal currentPrice = 1500m;
        const string expectedMessage = "Ціна не може бути 0 або менше";

        // Act
        Result result = service.ChangePrice(newPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        service.Price.Should().Be(currentPrice);
    }

    [Fact]
    public void GetFinalPrice_ShouldReturnOriginalPrice_WhenDiscountIsNull()
    {
        // Arrange
        Service service = CreateService();
        DateOnly currentDate = new(2026, 4, 1);

        // Act
        decimal result = service.GetFinalPrice(currentDate);

        // Assert
        result.Should().Be(service.Price);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void GetFinalPrice_ShouldReturnOriginalPrice_WhenDiscountIsInactiveForDate(int currentOffsetDays)
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, discount);
        Service service = createResult.Value!;
        DateOnly currentDate = new DateOnly(2026, 4, 1).AddDays(currentOffsetDays);

        // Act
        decimal result = service.GetFinalPrice(currentDate);

        // Assert
        result.Should().Be(service.Price);
    }

    [Theory]
    [InlineData(0, 1200)]
    [InlineData(3, 1200)]
    [InlineData(5, 1200)]
    public void GetFinalPrice_ShouldApplyDiscount_WhenDiscountIsActive(int currentOffsetDays, decimal expectedPrice)
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, discount);
        Service service = createResult.Value!;
        DateOnly currentDate = new DateOnly(2026, 4, 1).AddDays(currentOffsetDays);

        // Act
        decimal result = service.GetFinalPrice(currentDate);

        // Assert
        result.Should().Be(expectedPrice);
    }

    [Fact]
    public void GetFinalPrice_ShouldReturnOriginalPrice_WhenDiscountExistsButExpiredStatusIsSet()
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 1, percent: 20);
        Result expireResult = discount.Expire(new DateOnly(2026, 4, 3));
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, discount);
        Service service = createResult.Value!;

        // Act
        decimal result = service.GetFinalPrice(new DateOnly(2026, 4, 3));

        // Assert
        expireResult.IsSuccess.Should().BeTrue();
        result.Should().Be(service.Price);
    }

    [Fact]
    public void AddDiscount_ShouldReturnSuccess_WhenArgumentsAreValid()
    {
        // Arrange
        Service service = CreateService();
        DateOnly currentDate = new DateOnly(2026, 4, 1);
        DateOnly startDate = currentDate;
        DateOnly endDate = currentDate.AddDays(5);
        const uint percent = 20;

        // Act
        Result result = service.AddDiscount(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        service.Discount.Should().NotBeNull();
        service.Discount!.StartDate.Should().Be(startDate);
        service.Discount.EndDate.Should().Be(endDate);
        service.Discount.Percent.Should().Be(percent);
    }

    [Fact]
    public void AddDiscount_ShouldReplaceExistingDiscount_WhenArgumentsAreValid()
    {
        // Arrange
        Discount existingDiscount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 10);
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, existingDiscount);
        Service service = createResult.Value!;
        Discount originalDiscount = service.Discount!;
        DateOnly currentDate = new DateOnly(2026, 4, 1);
        DateOnly newStartDate = currentDate.AddDays(1);
        DateOnly newEndDate = currentDate.AddDays(3);
        const uint newPercent = 25;

        // Act
        Result result = service.AddDiscount(currentDate, newStartDate, newEndDate, newPercent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.Discount.Should().NotBeNull();
        service.Discount.Should().NotBeSameAs(originalDiscount);
        service.Discount!.StartDate.Should().Be(newStartDate);
        service.Discount.EndDate.Should().Be(newEndDate);
        service.Discount.Percent.Should().Be(newPercent);
    }

    [Fact]
    public void AddDiscount_ShouldReturnFailure_WhenDiscountArgumentsAreInvalid()
    {
        // Arrange
        Service service = CreateService();
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(-1);
        DateOnly endDate = currentDate.AddDays(5);
        const uint percent = 20;
        const string expectedMessage = "Дата початку знижки не може бути раніше ніж сьогодні";

        // Act
        Result result = service.AddDiscount(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        service.Discount.Should().BeNull();
    }

    [Fact]
    public void AddDiscount_ShouldKeepExistingDiscount_WhenDiscountArgumentsAreInvalid()
    {
        // Arrange
        Discount existingDiscount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 10);
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, existingDiscount);
        Service service = createResult.Value!;
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate;
        DateOnly endDate = currentDate.AddDays(5);
        const uint percent = 0;
        const string expectedMessage = "Відсоток знижки не може бути 0 або більше 100";

        // Act
        Result result = service.AddDiscount(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        service.Discount.Should().BeSameAs(existingDiscount);
    }

    [Fact]
    public void RemoveDiscount_ShouldSetDiscountToNull_WhenDiscountExists()
    {
        // Arrange
        Discount discount = CreateDiscount(startOffsetDays: 0, endOffsetDays: 5, percent: 20);
        Result<Service> createResult = Service.Create("Консультація", "Первинна консультація", 1500m, discount);
        Service service = createResult.Value!;

        // Act
        service.RemoveDiscount();

        // Assert
        service.Discount.Should().BeNull();
    }

    [Fact]
    public void RemoveDiscount_ShouldKeepDiscountNull_WhenDiscountDoesNotExist()
    {
        // Arrange
        Service service = CreateService();

        // Act
        service.RemoveDiscount();

        // Assert
        service.Discount.Should().BeNull();
    }

    private static Service CreateService()
        => Service.Create("Консультація", "Первинна консультація", 1500m).Value!;

    private static Discount CreateDiscount(int startOffsetDays, int endOffsetDays, uint percent)
    {
        // Arrange
        DateOnly currentDate = new(2026, 4, 1);
        DateOnly startDate = currentDate.AddDays(startOffsetDays);
        DateOnly endDate = currentDate.AddDays(endOffsetDays);

        // Act
        Result<Discount> result = Discount.Create(currentDate, startDate, endDate, percent);

        // Assert
        result.IsSuccess.Should().BeTrue();

        return result.Value!;
    }
}
