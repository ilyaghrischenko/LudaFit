using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Specialist : BaseEntity
{
    public string Name { get; private set; } = null!;

    public string PhotoUrl { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public TimeRange WorkTime { get; private set; } = null!;

    private Specialist() { }

    private Specialist(string name, string photoUrl, string description, TimeRange workTime)
    {
        Name = name;
        PhotoUrl = photoUrl;
        Description = description;
        WorkTime = workTime;
    }

    public static Result<Specialist> Create(
        string name,
        string photoUrl,
        string description,
        int startWorkHour,
        int startWorkMinute,
        int endWorkHour,
        int endWorkMinute)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Ім'я не може бути пустим");
        }
        
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return new ErrorDetails("Фотографія не може бути пустою");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис не може бути пустим");
        }

        Result<TimeRange> createWorkTimeResult = TimeRange.Create(startWorkHour, startWorkMinute, endWorkHour, endWorkMinute);

        if (createWorkTimeResult.IsFailure)
        {
            return Result<Specialist>.Failure(createWorkTimeResult);
        }

        return new Specialist(
            name,
            photoUrl,
            description,
            createWorkTimeResult.Value!
        );
    }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Ім'я не може бути пустим");
        }

        Name = name;
        return Result.Success();
    }

    public Result ChangePhotoUrl(string newPhotoUrl)
    {
        if (string.IsNullOrWhiteSpace(newPhotoUrl))
        {
            return new ErrorDetails("Нова фотографія не може бути пустою");
        }

        if (PhotoUrl == newPhotoUrl)
        {
            return new ErrorDetails("Нова фотографія повинна відрізнятися");
        }
        
        PhotoUrl = newPhotoUrl;
        return Result.Success();
    }

    public Result ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис не може бути пустим");
        }

        if (Description == description)
        {
            return new ErrorDetails("Новий опис повинен відрізнятися");
        }
        
        Description = description;
        return Result.Success();
    }

    public Result ChangeWorkTime(int startWorkHour, int startWorkMinute, int endWorkHour, int endWorkMinute)
    {
        Result<TimeRange> createNewWorkTimeResult = TimeRange.Create(startWorkHour, startWorkMinute, endWorkHour, endWorkMinute);

        if (createNewWorkTimeResult.IsFailure)
        {
            return createNewWorkTimeResult;
        }

        if (WorkTime == createNewWorkTimeResult.Value!)
        {
            return new ErrorDetails("Новий графік роботи повністю співпадає з поточним");
        }

        WorkTime = createNewWorkTimeResult.Value!;
        return Result.Success();
    }
}
