using System.Net.Mail;
using System.Text.RegularExpressions;
using LudaFit.Domain.Rules;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record ClientContacts
{
    public string PhoneNumber { get; init; } = null!;

    public MailAddress Email { get; init; } = null!;
    
    public string? TelegramTag { get; init; }
    
    private ClientContacts() { }

    private ClientContacts(string phoneNumber, MailAddress email, string? telegramTag)
    {
        PhoneNumber = phoneNumber;
        Email = email;
        TelegramTag = telegramTag;
    }

    public static Result<ClientContacts> Create(string phoneNumber, string email, string? telegramTag = null)
    {
        string phoneNumberTrim = phoneNumber.Trim();

        if (PhoneNumberRules.IsValid(phoneNumberTrim) is false)
        {
            return new ErrorDetails("Номер телефону вказаний не вірно");
        }

        if (!MailAddress.TryCreate(email.Trim(), out MailAddress? emailAddress))
        {
            return new ErrorDetails("Пошта вказана не вірно");
        }

        if (telegramTag is not null)
        {
            string telegramTagTrim = telegramTag.Trim();
            
            if (string.IsNullOrWhiteSpace(telegramTagTrim))
            {
                return new ErrorDetails("Тег телеграму не може бути пустий");
            }

            if (telegramTagTrim.First() != '@')
            {
                return new ErrorDetails("Тег телеграму повинен починатся з '@'");
            }
        }
        
        return new ClientContacts(phoneNumberTrim, emailAddress, telegramTag);
    }
}
