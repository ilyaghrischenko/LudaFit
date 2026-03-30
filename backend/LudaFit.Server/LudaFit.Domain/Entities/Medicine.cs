using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Medicine : BaseEntity
{
    public string Name { get; private set; }

    private Medicine()
    {
        Name = null!;
    }

    private Medicine(string name)
    {
        Name = name;
    }

    public static Result<Medicine> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва ліків не може бути пустою");
        }

        return new Medicine(name);
    }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва ліків не може бути пустою");
        }
        
        Name = name;
        return Result.Success();
    }
}
