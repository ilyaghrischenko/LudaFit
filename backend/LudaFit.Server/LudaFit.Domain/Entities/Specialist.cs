using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Specialist : BaseEntity
{
    public string PhotoUrl { get; private set; }
    
    public string Description { get; private set; }

    private Specialist()
    {
        PhotoUrl = null!;
        Description = null!;
    }

    private Specialist(string photoUrl, string description)
    {
        PhotoUrl = photoUrl;
        Description = description;
    }

    public static Result<Specialist> Create(string photoUrl, string description)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return new ErrorDetails("Фотографія не може бути пустою");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return new ErrorDetails("Опис не може бути пустим");
        }

        return new Specialist(
            photoUrl,
            description
        );
    }

    public Result ChangePhotoUrl(string newPhotoUrl)
    {
        if (string.IsNullOrWhiteSpace(newPhotoUrl))
        {
            return new ErrorDetails("Нова фотографія не може бути пустою");
        }

        if (PhotoUrl == newPhotoUrl)
        {
            return new ErrorDetails("Нова фотографія повина відрізнятися");
        }
        
        PhotoUrl = newPhotoUrl;
        return Result.Success();
    }

    public Result ChangeDecription(string description)
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
}
