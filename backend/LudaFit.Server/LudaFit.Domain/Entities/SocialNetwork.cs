using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class SocialNetwork : BaseEntity
{
    public string Name { get; private set; } = null!;

    public string Url { get; private set; } = null!;

    public string PhotoUrl { get; private set; } = null!;

    public Specialist Specialist { get; private set; } = null!;
    public int SpecialistId { get; private set; }

    private SocialNetwork() { }

    private SocialNetwork(string name, string url, string photoUrl, Specialist specialist)
    {
        Name = name;
        Url = url;
        PhotoUrl = photoUrl;
        Specialist = specialist;
        SpecialistId = specialist.Id;
    }

    public static Result<SocialNetwork> Create(string name, string url, string photoUrl, Specialist specialist)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва соціальної мережі не може бути пустою");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return new ErrorDetails("Посилення на соціальну мережу не може бути пустим");
        }

        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return new ErrorDetails("Фото не може бути пустим");
        }

        return new SocialNetwork(name.Trim(), url.Trim(), photoUrl.Trim(), specialist);
    }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва соціальної мережі не може бути пустою");
        }

        if (Name.Trim() == name.Trim())
        {
            return new ErrorDetails("Назва соціальної мережі має так ж саму назву");
        }
        
        Name = name;
        return Result.Success();
    }

    public Result ChangeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new ErrorDetails("Посилання на соціальну мережу не може бути пустим");
        }

        if (Url.Trim() == url.Trim())
        {
            return new ErrorDetails("Неможливо змінити посилання на так ж саме");
        }
        
        Url = url;
        return Result.Success();
    }

    public Result ChangePhotoUrl(string photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return new ErrorDetails("Посилання на фото не модже бути пустим");
        }

        if (PhotoUrl.Trim() == photoUrl.Trim())
        {
            return new ErrorDetails("Неможливо змінити посилання на фото на так ж саме");
        }
        
        PhotoUrl = photoUrl;
        return Result.Success();
    }
}
