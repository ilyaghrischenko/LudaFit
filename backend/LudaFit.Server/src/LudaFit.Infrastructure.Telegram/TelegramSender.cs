using System.Globalization;
using System.Net;
using System.Text;
using LudaFit.Infrastructure.Telegram.Exceptions;
using LudaFit.Infrastructure.Telegram.Options;
using LudaFit.Infrastructure.Telegram.Settings;
using LudaFit.SharedKernel.Interfaces;
using LudaFit.SharedKernel.Models;
using LudaFit.SharedKernel.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LudaFit.Infrastructure.Telegram;

//todo: написать заметку об этом в обсидиан
public sealed partial class TelegramSender(
    IOptions<TelegramSettings> options,
    ITelegramBotClient telegramBotClient,
    ILogger<TelegramSender> logger) : IScopedType
{
    private readonly TelegramSettings _settings = options.Value;
    
    [LoggerMessage(1, LogLevel.Error, "Error while sending telegram message")]
    private partial void LogTelegramError(Exception ex);

    public async Task<Result> SendAsync(SendTelegramOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ClientName)
            || string.IsNullOrWhiteSpace(options.ClientEmail)
            || string.IsNullOrWhiteSpace(options.ClientPhoneNumber))
        {
            return new ErrorDetails("Ім'я або пошта або номер телефона клієнта не можуть бути пустими");
        }

        StringBuilder stringBuilder = new($"""
            <b>Нова заявка на запис</b>
            <i><strong>Ім'я клієнта:</strong> {options.ClientName}</i>
            <i><strong>Email:</strong> {options.ClientEmail}</i>
            <i><strong>Телефон:</strong> {options.ClientPhoneNumber}</i>
            {(options.ClientTelegramTag != null ? $"<i><strong>Telegram:</strong> {options.ClientTelegramTag}</i>" : string.Empty)}
         
            <i><strong>Назва послуги:</strong> {options.Booking.ServiceName}</i>
            <i><strong>Ціна послуги:</strong> {options.Booking.ServicePrice}</i>
            <i><strong>Вік:</strong> {options.Booking.Age}</i>
            <i><strong>Зріст:</strong> {options.Booking.Height}</i>
            <i><strong>Вага:</strong> {options.Booking.Weight}</i>
            <i><strong>Обхват таліїі:</strong> {options.Booking.WaistSize}</i>
            <i><strong>Ціль:</strong> {options.Booking.Purpose}</i>
         
            
        """);

        string textHtml = BuildEmailMessage(stringBuilder, options.Booking);

        try
        {
            await telegramBotClient.SendMessage(
                chatId: _settings.ChatId,
                text: textHtml,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            LogTelegramError(ex);
            throw new SendTelegramMessageException(ex);
        }
        
        return Result.Success();
    }

    private static string BuildEmailMessage(StringBuilder stringBuilder, BookingOptions bookingOptions)
    {
        if (bookingOptions.PhysicalActivities != null && !string.IsNullOrWhiteSpace(bookingOptions.PhysicalActivities))
        {
            stringBuilder.AppendLine(
                CultureInfo.InvariantCulture,
                $"<i><strong>Фізична активність:</strong> {bookingOptions.PhysicalActivities}</i>"
            );
        }

        if (bookingOptions.ClientAdditionalInformation != null)
        {
            if (bookingOptions.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Скарги на самопочуття:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.FeelingUnwellComplaints}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.Allergies != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Алергії:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.Allergies}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.Intolerances != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Не переносимості їжі:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.Intolerances}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Стресс та як ви справляєтся з ним:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.StressAndHowYouCopeWithIt}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientHealth.AnxietyTendency != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Чи є тривожність:</strong> {bookingOptions.ClientAdditionalInformation.ClientHealth.AnxietyTendency}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Улюблена їжа:</strong> {bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.FavoriteFoods}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Не улюблена їжа:</strong> {bookingOptions.ClientAdditionalInformation.ClientFoodPreferences.UnfavoriteFoods}</i>"
                );
            }

            if (bookingOptions.ClientAdditionalInformation.FoodWeighing != null)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<i><strong>Чи зважуєте (або хочете зважувати) їжу:</strong> {bookingOptions.ClientAdditionalInformation.FoodWeighing}</i>"
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
                $"<i><strong>Назва діагнозу:</strong> {diagnosis.Name}</i>"
            );

            foreach (MedicineOptions medicine in diagnosis.Medicines)
            {
                stringBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"\t<i><strong>Назва ліків:</strong> {medicine.Name}</i>"
                );
            }

            stringBuilder.AppendLine("\n");
        }

        return stringBuilder.ToString();
    }
}
