using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.Enums;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Discount : BaseEntity
{
    public DateOnly StartDate { get; init; }
    
    public DateOnly EndDate { get; private set; }
    
    public uint Percent { get; private set; }
    
    public DiscountStatus Status { get; private set; } = DiscountStatus.Active;

    private Discount() { }

    private Discount(DateOnly startDate, DateOnly endDate, uint percent)
    {
        StartDate = startDate;
        EndDate = endDate;
        Percent = percent;
    }
    
    //todo: написать фоновый процесс который будет отлавливать все не активные скидки и удалять их
    // может тогда вообще убрать статус?

    public static Result<Discount> Create(DateOnly currentDate, DateOnly startDate, DateOnly endDate, uint percent)
    {
        if (startDate < currentDate)
        {
            return new ErrorDetails("Дата початку знижки не може бути раніше ніж сьогодні");
        }

        if (endDate < currentDate)
        {
            return new ErrorDetails("Дата кінця знижки не може бути раніше ніж сьогодні");
        }

        if (startDate > endDate)
        {
            return new ErrorDetails("Дата початку знижки не може бути пізніше ніж дата її кінця");
        }

        if (percent is > 100 or 0)
        {
            return new ErrorDetails("Відсоток знижки не може бути 0 або більше 100");
        }

        return new Discount(
            startDate,
            endDate,
            percent
        );
    }
    
    public bool IsActive(DateOnly currentDate)
        => Status == DiscountStatus.Active
           && currentDate >= StartDate
           && currentDate <= EndDate;

    public Result ChangeEndDate(DateOnly currentDate, DateOnly endDate)
    {
        if (Status == DiscountStatus.Expired)
        {
            return new ErrorDetails("Неможливо змінити дату кінця знижки, вона вже скінчилась");
        }
        
        if (endDate < currentDate)
        {
            return new ErrorDetails("Дата кінця знижки не може бути раніше ніж сьогодні");
        }
        
        if (StartDate > endDate)
        {
            return new ErrorDetails("Дата початку знижки не може бути пізніше ніж дата її кінця");
        }
        
        EndDate = endDate;
        return Result.Success();
    }

    public Result ChangePercent(uint percent)
    {
        if (Status == DiscountStatus.Expired)
        {
            return new ErrorDetails("Неможливо змінити відсоток знижки, вона вже скінчилась");
        }
        
        if (percent is > 100 or 0)
        {
            return new ErrorDetails("Відсоток знижки не може бути 0 або більше 100");
        }
        
        Percent = percent;
        return Result.Success();
    }

    public Result Expire(DateOnly currentDate)
    {
        if (Status == DiscountStatus.Expired)
        {
            return Result.Success();
        }
        
        if (EndDate >= currentDate)
        {
            return new ErrorDetails("Знижка ще активна, її не можливо позначити як закінчену");
        }
        
        Status = DiscountStatus.Expired;
        return Result.Success();
    }

    public Result<decimal> CalculatePrice(DateOnly currentDate, decimal price)
    {
        if (!IsActive(currentDate))
        {
            return new ErrorDetails("розрахувати ціну неможливо, знижка не активна");
        }
        
        return price * (1 - Percent / 100m);
    }
}
