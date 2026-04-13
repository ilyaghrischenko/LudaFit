using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Service : BaseEntity
{
    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;
    
    public decimal Price { get; private set; }
    
    public Discount? Discount { get; private set; }
    public int? DiscountId { get; private set; }
    
    private Service() { }

    private Service(string name, string description, decimal price, Discount? discount = null)
    {
        Name = name;
        Description = description;
        Price = price;
        Discount = discount;
        DiscountId = discount?.Id;
    }

    public static Result<Service> Create(string name, string description, decimal price, Discount? discount = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва послуги не може бути пустою");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис послуги не може бути пустим");
        }

        if (price <= 0)
        {
            return new ErrorDetails("Ціна не може бути 0 або менше");
        }

        return new Service(
            name,
            description,
            price,
            discount
        );
    }

    public decimal GetFinalPrice(DateOnly currentDate)
    {
        if (Discount is null)
        {
            return Price;
        }

        if (Discount.IsActive(currentDate) is false)
        {
            return Price;
        }
        
        return Price * (1 - Discount!.Percent / 100m);
    }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва послуги не може бути пустою");
        }
        
        Name = name;
        return Result.Success();
    }

    public Result ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис послуги не може бути пустим");
        }
        
        Description = description;
        return Result.Success();
    }

    public Result ChangePrice(decimal price)
    {
        if (price <= 0)
        {
            return new ErrorDetails("Ціна не може бути 0 або менше");
        }
        
        Price = price;
        return Result.Success();
    }

    public Result AddDiscount(DateRange dateRange, uint percent)
    {
        Result<Discount> createDiscountResult = Discount.Create(dateRange, percent);

        if (createDiscountResult.IsFailure)
        {
            return createDiscountResult;
        }
        
        Discount = createDiscountResult.Value;
        return Result.Success();
    }

    public void RemoveDiscount()
        => Discount = null;
}
