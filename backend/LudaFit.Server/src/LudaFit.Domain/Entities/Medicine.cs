using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Medicine : BaseEntity
{
    public string Name { get; private set; } = null!;

    public Diagnosis Diagnosis { get; private set; } = null!;
    public int DiagnosisId { get; private set; }

    private Medicine() { }

    private Medicine(string name, Diagnosis diagnosis)
    {
        Name = name;
        Diagnosis = diagnosis;
        DiagnosisId = diagnosis.Id;
    }

    public static Result<Medicine> Create(string name, Diagnosis diagnosis)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва ліків не може бути пустою");
        }

        return new Medicine(name, diagnosis);
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
