using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record TimeRange
{
    public TimeOnly Start { get; init; }
    
    public TimeOnly End { get; init; }

    private TimeRange() { }

    private TimeRange(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    public static Result<TimeRange> Create(int startHour, int startMinute, int endHour, int endMinute)
    {
        if (startHour < 0 || endHour < 0)
        {
            return new ErrorDetails("Година початку або кінця не може бути від'ємною");
        }

        if (startHour > 23 || endHour > 23)
        {
            return new ErrorDetails("Година початку або кінця не може бути більше 23");
        }

        if (startMinute < 0 || endMinute < 0)
        {
            return new ErrorDetails("Хвилина початку або кінця не може бути від'ємною");
        }

        if (startMinute > 59 || endMinute > 59)
        {
            return new ErrorDetails("Хвилина початку або кінця не може бути більше 59");
        }

        if (startHour > endHour)
        {
            return new ErrorDetails("Година початку не може бути більшою за годину кінця");
        }

        if (startHour == endHour)
        {
            if (startMinute >= endMinute)
            {
                return new ErrorDetails("Якщо години початку і кінця рівні, то хвилина початку"
                                        + " не може бути більшою за хвилину кінця або дорівнюват їй");
            }
        }

        TimeOnly start = new(startHour, startMinute);
        TimeOnly end = new(endHour, endMinute);

        return new TimeRange(start, end);
    }
}
