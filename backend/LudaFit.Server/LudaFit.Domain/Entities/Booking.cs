using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public Guid IdempotencyKey { get; private set; }
    
    public string ServiceName { get; private set; } = null!;
    public decimal ServicePrice { get; private set; }
    public int ServiceId { get; init; }

    public string FullName { get; private set; } = null!;

    public ClientMetrics ClientMetrics { get; private set; } = null!;

    public string Purpose { get; private set; } = null!;

    public string? PhysicalActivities { get; private set; }
    
    public ClientContacts ClientContacts { get; private set; } = null!;

    public ClientAdditionalInformation? ClientAdditionalInformation { get; private set; }
    
    private readonly List<Diagnosis> _diagnoses = [];
    public IReadOnlyCollection<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();
    
    private Booking() { }

    private Booking(
        Guid idempotencyKey,
        string serviceName,
        decimal servicePrice,
        int serviceId,
        string fullName,
        ClientMetrics clientMetrics,
        string purpose,
        ClientAdditionalInformation? clientAdditionalInformation,
        ClientContacts clientContacts,
        string? physicalActivities = null)
    {
        IdempotencyKey = idempotencyKey;
        ServiceName = serviceName;
        ServicePrice = servicePrice;
        ServiceId = serviceId;
        FullName = fullName;
        ClientMetrics = clientMetrics;
        Purpose = purpose;
        ClientAdditionalInformation = clientAdditionalInformation;
        PhysicalActivities = physicalActivities;
        ClientContacts = clientContacts;
    }

    public static Result<Booking> Create(
        Guid idempotencyKey,
        string serviceName,
        decimal servicePrice,
        int serviceId,
        string fullName,
        ClientMetrics clientMetrics,
        string purpose,
        ClientContacts clientContacts,
        string? physicalActivities = null,
        CreateDiagnosisForBooking[]? diagnosesForBooking = null,
        ClientAdditionalInformation? clientAdditionalInformation = null)
    {
        if (idempotencyKey == Guid.Empty)
        {
            return new ErrorDetails("Не вірний формат ключа");
        }
        
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return new ErrorDetails("Назва послуги не може бути пустою");
        }

        if (servicePrice <= 0)
        {
            return new ErrorDetails("Ціна не може бути 0 або менше");
        }

        if (serviceId == 0)
        {
            return new ErrorDetails("Айді послуги не може бути 0");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return new ErrorDetails("ПІБ не може бути пустим");
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            return new ErrorDetails("Мета схуднення не може бути пустою");
        }
        
        if (physicalActivities is not null && string.IsNullOrWhiteSpace(physicalActivities))
        {
            return new ErrorDetails("Фізична активність не може бути пустою");
        }

        Booking booking = new(
            idempotencyKey,
            serviceName,
            servicePrice,
            serviceId,
            fullName,
            clientMetrics,
            purpose,
            clientAdditionalInformation,
            clientContacts,
            physicalActivities
        );
        
        if (diagnosesForBooking is null || diagnosesForBooking.Length == 0)
        {
            return booking;
        }
        
        Result addDiagnosesResult = booking.AddDiagnoses(diagnosesForBooking);

        if (addDiagnosesResult.IsFailure)
        {
            return Result<Booking>.Failure(addDiagnosesResult);
        }

        return booking;
    }
    
    private Result AddDiagnoses(CreateDiagnosisForBooking[] diagnosesForBooking)
    {
        List<Diagnosis> diagnoses = new(diagnosesForBooking.Length);
        
        foreach (CreateDiagnosisForBooking diagnosisForBooking in diagnosesForBooking)
        {
            bool alreadyExists = _diagnoses.Any(existing => existing.Name.Trim().Equals(diagnosisForBooking.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                                 || diagnoses.Any(existing => existing.Name.Trim().Equals(diagnosisForBooking.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                return new ErrorDetails(
                    $"Діагноз з назвою {diagnosisForBooking.Name} вже існує",
                    HttpStatusCode.Conflict
                );
            }
            
            Result<Diagnosis> createDiagnosisForBooking = Diagnosis.Create(diagnosisForBooking.Name, this, diagnosisForBooking.MedicineNames);

            if (createDiagnosisForBooking.IsFailure)
            {
                return createDiagnosisForBooking;
            }
            
            diagnoses.Add(createDiagnosisForBooking.Value!);
        }
        
        _diagnoses.AddRange(diagnoses);
        return Result.Success();
    }
}

#pragma warning disable SA1402
public sealed record CreateDiagnosisForBooking(
    string Name,
    IReadOnlyCollection<string> MedicineNames
);
#pragma warning restore SA1402
