using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record DateRange
{
    public DateOnly Start { get; init; }
    
    public DateOnly End { get; init; }

    private DateRange() { }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static Result<DateRange> Create(DateOnly currentDate, DateOnly start, DateOnly end)
    {
        if (start < currentDate)
        {
            return new ErrorDetails("Дата початку знижки не може бути раніше ніж сьогодні");
        }

        if (end < currentDate)
        {
            return new ErrorDetails("Дата кінця знижки не може бути раніше ніж сьогодні");
        }

        if (start > end)
        {
            return new ErrorDetails("Дата початку знижки не може бути пізніше ніж дата її кінця");
        }

        return new DateRange(start, end);
    }
}
