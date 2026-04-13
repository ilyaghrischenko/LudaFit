using System.Globalization;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.Gmail.Settings;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using LudaFit.SharedKernel.Options;
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
            || string.IsNullOrWhiteSpace(options.ClientPhoneNumber))
        {
            return new ErrorDetails("Ім'я або пошта або номер телефона клієнта не можуть бути пустими");
        }
        
        using MimeMessage mimeMessage = new();
        
        mimeMessage.From.Add(new MailboxAddress(_settings.OrganisationName, _settings.OrganisationEmail));
        mimeMessage.To.Add(new MailboxAddress(_settings.SpecialistName, _settings.SpecialistEmail));
        mimeMessage.ReplyTo.Add(new MailboxAddress(options.ClientName, options.ClientEmail));

        mimeMessage.Subject = $"Нова заявка з сайту від: {options.ClientName}";
        
        StringBuilder stringBuilder = new($"""
            <h2>Нова заявка на запис</h2>
            <p><strong>Ім'я клієнта:</strong> {options.ClientName}</p>
            <p><strong>Email:</strong> {options.ClientEmail}</p>
            <p><strong>Телефон:</strong> {options.ClientPhoneNumber}</p>
            {(options.ClientTelegramTag != null ? $"<p><strong>Telegram:</strong> {options.ClientTelegramTag}</p>" : string.Empty)}
            
            <p><strong>Назва послуги:</strong> {options.Booking.ServiceName}</p>
            <p><strong>Ціна послуги:</strong> {options.Booking.ServicePrice}</p>
            <p><strong>Вік:</strong> {options.Booking.Age}</p>
            <p><strong>Зріст:</strong> {options.Booking.Height}</p>
            <p><strong>Вага:</strong> {options.Booking.Weight}</p>
            <p><strong>Обхват таліїі:</strong> {options.Booking.WaistSize}</p>
            <p><strong>Ціль:</strong> {options.Booking.Purpose}</p>

            
        """);

        string bodyHtml = BuildEmailMessage(stringBuilder, options.Booking);

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
#pragma warning disable CA1031
        catch
#pragma warning restore CA1031
        {
            return Result.Failure(
                "Не вдалося відправити листа на пошту",
                HttpStatusCode.InternalServerError
            );
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
        
        return Result.Success();
    }

    private static string BuildEmailMessage(StringBuilder stringBuilder, BookingOptions bookingOptions)
    {
        if (bookingOptions.PhysicalActivities != null && !string.IsNullOrWhiteSpace(bookingOptions.PhysicalActivities))
        {
            stringBuilder.AppendLine(
                CultureInfo.InvariantCulture,
                $"<p><strong>Фізична активність:</strong> {bookingOptions.PhysicalActivities}</p>"
            );
        }

        if (bookingOptions.ClientAdditionalInformation != null)
        {
            if (bookingOptions.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Скарги на самопочуття:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints}</p>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.Allergies != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Алергії:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.Allergies}</p>"
                );
            }
            
            if (bookingOptions.ClientAdditionalInformation.ClientHealth.Intolerances != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Не переносимості їжі:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.Intolerances}</p>"
                );
            }
            
            if (bookingOptions.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Стресс та як ви справляєтся з ним:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt}</p>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.AnxietyTendency != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Чи є тривожність:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.AnxietyTendency}</p>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Улюблена їжа:</strong> {bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods}</p>"
                );
            }
            
            if (bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Не улюблена їжа:</strong> {bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods}</p>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.FoodWeighing != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<p><strong>Чи зважуєте (або хочете зважувати) їжу:</strong> {bookingOptions.ClientAdditionalInformation.FoodWeighing}</p>"
                );
            }
        }

        if (bookingOptions.Diagnoses.Count == 0)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine("\n");
        
        foreach (DiagnosisOptions diagnosis in bookingOptions.Diagnoses)
        {
            stringBuilder.AppendLine(
                CultureInfo.InvariantCulture,
                $"<p><strong>Назва діагнозу:</strong> {diagnosis.Name}</p>"
            );

            foreach (MedicineOptions medicine in diagnosis.Medicines)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"\t<p><strong>Назва ліків:</strong> {medicine.Name}</p>"
                );
            }

            stringBuilder.AppendLine("\n");
        }
        
        return stringBuilder.ToString();
    }
}
