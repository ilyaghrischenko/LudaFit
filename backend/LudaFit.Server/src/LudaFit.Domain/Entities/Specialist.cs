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

    private readonly List<SocialNetwork> _socialNetworks = [];
    public IReadOnlyCollection<SocialNetwork> SocialNetworks => _socialNetworks.AsReadOnly();

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
        TimeRange workTime)
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

        return new Specialist(
            name,
            photoUrl,
            description,
            workTime
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

    public Result AddSocialNetwork(string name, string url, string photoUrl)
    {
        bool alreadyExists = _socialNetworks.Any(sn => sn.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return new ErrorDetails(
                "Соціальна мережа з такою назвою вже існує",
                HttpStatusCode.Conflict
            );
        }
        
        Result<SocialNetwork> createSocialNetworkResult = SocialNetwork.Create(name, url, photoUrl, this);

        if (createSocialNetworkResult.IsFailure)
        {
            return createSocialNetworkResult;
        }
        
        _socialNetworks.Add(createSocialNetworkResult.Value!);
        return Result.Success();
    }

    public Result AddSocialNetworks(CreateSocialNetworkForSpecialist[] socialNetworks)
    {
        List<SocialNetwork> newSocialNetworks = new(socialNetworks.Length);
        
        foreach (var createSocialNetworkForSpecialist in socialNetworks)
        {
            bool alreadyExists = _socialNetworks.Any(sn => sn.Name.Equals(createSocialNetworkForSpecialist.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                return new ErrorDetails(
                    "Соціальна мережа з такою назвою вже існує",
                    HttpStatusCode.Conflict
                );
            }

            Result<SocialNetwork> createSocialNetworkResult = SocialNetwork.Create(
                createSocialNetworkForSpecialist.Name,
                createSocialNetworkForSpecialist.Url,
                createSocialNetworkForSpecialist.PhotoUrl,
                this
            );

            if (createSocialNetworkResult.IsFailure)
            {
                return createSocialNetworkResult;
            }
            
            newSocialNetworks.Add(createSocialNetworkResult.Value!);
        }

        _socialNetworks.AddRange(newSocialNetworks);
        return Result.Success();
    }
}

#pragma warning disable SA1402
public sealed record CreateSocialNetworkForSpecialist(string Name, string Url, string PhotoUrl);
#pragma warning restore SA1402
