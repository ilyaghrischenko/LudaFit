using System.Net.Mail;
using System.Text.RegularExpressions;
using LudaFit.Domain.Rules;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record ClientContacts
{
    public string PhoneNumber { get; init; } = null!;

    public MailAddress Email { get; init; } = null!;

    private ClientContacts() { }

    private ClientContacts(string phoneNumber, MailAddress email)
    {
        PhoneNumber = phoneNumber;
        Email = email;
    }

    public static Result<ClientContacts> Create(string phoneNumber, string email)
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
        
        return new ClientContacts(phoneNumberTrim, emailAddress);
    }
}
