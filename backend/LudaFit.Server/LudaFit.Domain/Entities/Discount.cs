using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.Enums;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Discount : BaseEntity
{
    public DateRange DateRange { get; private set; } = null!;
    
    public uint Percent { get; private set; }

    private readonly List<Service> _services = [];
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();
    
    public DiscountStatus Status { get; private set; } = DiscountStatus.Active;

    private Discount() { }

    private Discount(DateRange dateRange, uint percent)
    {
        DateRange = dateRange;
        Percent = percent;
    }
    
    //todo: написать фоновый процесс который будет отлавливать все не активные скидки и удалять их
    // может тогда вообще убрать статус?

    public static Result<Discount> Create(DateOnly currentDate, DateOnly startDate, DateOnly endDate, uint percent)
    {
        Result<DateRange> createDateRangeResult = DateRange.Create(currentDate, startDate, endDate);

        if (createDateRangeResult.IsFailure)
        {
            return Result<Discount>.Failure(createDateRangeResult);
        }

        if (percent is > 100 or 0)
        {
            return new ErrorDetails("Відсоток знижки не може бути 0 або більше 100");
        }

        return new Discount(
            createDateRangeResult.Value!,
            percent
        );
    }
    
    public bool IsActive(DateOnly currentDate)
        => Status == DiscountStatus.Active
           && currentDate >= DateRange.Start
           && currentDate <= DateRange.End;

    public Result ChangeEndDate(DateOnly currentDate, DateOnly endDate)
    {
        if (Status == DiscountStatus.Expired)
        {
            return new ErrorDetails("Неможливо змінити дату кінця знижки, вона вже скінчилась");
        }
        
        Result<DateRange> createDateRangeResult = DateRange.Create(currentDate, DateRange.Start, endDate);

        if (createDateRangeResult.IsFailure)
        {
            return createDateRangeResult;
        }

        DateRange = createDateRangeResult.Value!;
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
        
        if (DateRange.End >= currentDate)
        {
            return new ErrorDetails("Знижка ще активна, її не можливо позначити як закінчену");
        }
        
        Status = DiscountStatus.Expired;
        return Result.Success();
    }

    public Result AddService(Service service)
    {
        bool alreadyExists = _services.Any(existing => existing.Name.Trim().Equals(service.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return new ErrorDetails(
                $"Сервіс з назвою {service.Name} вже існує",
                HttpStatusCode.Conflict
            );
        }
        
        _services.Add(service);
        return Result.Success();
    }

    public Result AddServices(Service[] services)
    {
        foreach (Service service in services)
        {
            Result addServiceResult = AddService(service);

            if (addServiceResult.IsFailure)
            {
                return addServiceResult;
            }
        }
        
        return Result.Success();
    }
}
