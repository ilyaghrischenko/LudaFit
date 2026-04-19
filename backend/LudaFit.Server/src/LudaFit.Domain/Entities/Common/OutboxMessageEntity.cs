using System.Runtime.CompilerServices;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities.Common;

//todo: написать заметку про идемпотентность и аутбокс и внести туда эту сущность как пример хорошего базового класса
public abstract class OutboxMessageEntity(DateTime currentDateTime) : BaseEntity
{
    public int AttemptsCount { get; protected set; }

    public virtual int MaxAttemptNumber { get; } = 5;
    
    public bool UsedAllAttempts => AttemptsCount == MaxAttemptNumber;

    public DateTime NextAttemptAtUtc { get; protected set; } = currentDateTime;

    public virtual bool IsAnotherAttemptAvailable()
    {
        if (UsedAllAttempts)
        {
            return false;
        }
        
        AttemptsCount += 1;

        NextAttemptAtUtc = AttemptsCount switch
        {
            1 => NextAttemptAtUtc.AddMinutes(15),
            2 => NextAttemptAtUtc.AddHours(1),
            3 => NextAttemptAtUtc.AddHours(5),
            4 => NextAttemptAtUtc.AddDays(1),
            _ => NextAttemptAtUtc
        };
        
        return true;
    }
}
