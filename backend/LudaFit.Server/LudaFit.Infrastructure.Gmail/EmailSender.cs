using System.Runtime.InteropServices.ComTypes;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.Gmail.Settings;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LudaFit.Infrastructure.Gmail;

public sealed class EmailSender(IOptions<EmailSettings> options) : IScopedType
{
    private readonly EmailSettings _settings = options.Value;
    
    //todo: повесить рейт лимитер на ендпоинт который это использует + каптчу на фронте + написать заметку об этом в обсидиан
    public async Task<Result> SendAsync(SendEmailOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ClientName)
            || string.IsNullOrWhiteSpace(options.ClientEmail)
            || string.IsNullOrWhiteSpace(options.ClientPhoneNumber)
            || string.IsNullOrWhiteSpace(options.Message))
        {
            return new ErrorDetails("Пошта клієнта та тіло повідомлення не може бути пустим");
        }
        
        using MimeMessage mimeMessage = new();
        
        mimeMessage.From.Add(new MailboxAddress(_settings.OrganisationName, _settings.OrganisationEmail));
        mimeMessage.To.Add(new MailboxAddress(_settings.SpecialistName, _settings.SpecialistEmail));
        mimeMessage.ReplyTo.Add(new MailboxAddress(options.ClientName, options.ClientEmail));

        mimeMessage.Subject = $"Нова заявка з сайту від: {options.ClientName}";

        var bodyHtml = $@"
            <h2>Нова заявка на запис</h2>
            <p><strong>Ім'я клієнта:</strong> {options.ClientName}</p>
            <p><strong>Email:</strong> {options.ClientEmail}</p>
            <p><strong>Телефон:</strong> {options.ClientPhoneNumber}</p>
            <p><strong>Повідомлення:</strong> {options.Message}</p>
        ";

        mimeMessage.Body = new TextPart("html")
        {
            Text = bodyHtml
        };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(_settings.OrganisationEmail, _settings.Password, cancellationToken);
            await client.SendAsync(mimeMessage, cancellationToken);
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
        
        return Result.Success();
    }
}
