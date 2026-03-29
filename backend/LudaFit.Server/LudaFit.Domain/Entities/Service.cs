using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Service : BaseEntity
{
    public string Name { get; private set; }
    
    public string Description { get; private set; }
    
    public decimal Price { get; private set; }
    
    public Discount? Discount { get; private set; }
    
    public decimal FinalPrice { get; private set; }
    
    private Service()
    {
        Name = null!;
        Description = null!;
    }

    private Service(string name, string description, decimal price, decimal finalPrice, Discount? discount = null)
    {
        Name = name;
        Description = description;
        Price = price;
        FinalPrice = finalPrice;
        Discount = discount;
    }

    public static Result<Service> Create(string name, string description, decimal price, DateOnly currentDate, Discount? discount = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва послуги не може бути пустою");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис послуги не може буии пустим");
        }

        if (price <= 0)
        {
            return new ErrorDetails("Ціна не може бути 0 або менше");
        }
        
        decimal finalPrice = price;

        if (discount is not null)
        {
            Result<decimal> calculationResult = discount.CalculatePrice(currentDate, price);

            if (calculationResult.IsFailure)
            {
                return new ErrorDetails(
                    calculationResult.ErrorDetails!.ErrorMessage,
                    calculationResult.ErrorDetails!.StatusCode
                );
            }
            
            finalPrice = calculationResult.Value;
        }

        return new Service(
            name,
            description,
            price,
            finalPrice,
            discount
        );
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
            return new ErrorDetails("Опис послуги не может бути пустим");
        }
        
        Description = description;
        return Result.Success();
    }

    public Result ChangePrice(DateOnly currentDate, decimal price)
    {
        if (price <= 0)
        {
            return new ErrorDetails("Ціна не може бути 0 або менше");
        }
        
        Price = price;

        if (Discount is not null)
        {
            Result<decimal> calculationResult = Discount.CalculatePrice(currentDate, price);

            if (calculationResult.IsFailure)
            {
                return calculationResult;
            }
            
            FinalPrice = calculationResult.Value;
        }
        
        return Result.Success();
    }

    public Result AddDiscount(DateOnly currentDate, DateOnly startDate, DateOnly endDate, uint percent)
    {
        Result<Discount> createDiscountResult = Discount.Create(currentDate, startDate, endDate, percent);

        if (createDiscountResult.IsFailure)
        {
            return createDiscountResult;
        }
        
        Discount = createDiscountResult.Value;
        
        Result<decimal> calculationResult = Discount!.CalculatePrice(currentDate, Price);

        if (calculationResult.IsFailure)
        {
            return calculationResult;
        }
            
        FinalPrice = calculationResult.Value;
        return Result.Success();
    }

    public void RemoveDiscount()
    {
        Discount = null;
        FinalPrice = Price;
    }
    
    //todo: дать джемини проверить логику и валидацию + исправить красные модели в схеме обсидиан
}
