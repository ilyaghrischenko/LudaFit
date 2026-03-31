using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record ClientMetrics
{
    public uint Age { get; init; }
    
    public uint Height { get; init; }
    
    public float Weight { get; init; }
    
    public uint WaistSize { get; init; }

    private ClientMetrics() { }

    private ClientMetrics(uint age, uint height, float weight, uint waistSize)
    {
        Age = age;
        Height = height;
        Weight = weight;
        WaistSize = waistSize;
    }

    public static Result<ClientMetrics> Create(uint age, uint height, float weight, uint waistSize)
    {
        if (age is > 100 or 0)
        {
            return new ErrorDetails($"Дуже сумніваюся що тобі {age} років");
        }

        if (height is > 250 or 0)
        {
            return new ErrorDetails($"Дуже сумніваюся що ти {height}см зростом");
        }

        if (weight <= 0)
        {
            return new ErrorDetails("Вага не може бути від'ємною");
        }

        if (waistSize == 0)
        {
            return new ErrorDetails("Обхват талії не може бути 0");
        }

        return new ClientMetrics(age, height, weight, waistSize);
    }
}
