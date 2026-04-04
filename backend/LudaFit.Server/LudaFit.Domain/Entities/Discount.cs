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

    private Discount() { }

    private Discount(DateRange dateRange, uint percent)
    {
        DateRange = dateRange;
        Percent = percent;
    }
    
    public static Result<Discount> Create(DateRange dateRange, uint percent)
    {
        if (percent is > 100 or 0)
        {
            return new ErrorDetails("Відсоток знижки не може бути 0 або більше 100");
        }

        return new Discount(
            dateRange,
            percent
        );
    }
    
    public bool IsActive(DateOnly currentDate)
        => currentDate >= DateRange.Start
           && currentDate <= DateRange.End;

    public Result ChangeEndDate(DateOnly currentDate, DateOnly endDate)
    {
        if (currentDate > DateRange.End)
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

    public Result ChangePercent(DateOnly currentDate, uint percent)
    {
        if (currentDate > DateRange.End)
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
