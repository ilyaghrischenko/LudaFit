using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public string ServiceName { get; private set; } = null!;
    public decimal ServicePrice { get; private set; }
    public int ServiceId { get; init; }

    public string FullName { get; private set; } = null!;

    public ClientMetrics ClientMetrics { get; private set; } = null!;

    public string Purpose { get; private set; } = null!;

    private readonly List<Diagnosis> _diagnoses = new();
    public IReadOnlyCollection<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();

    public HealthQuestionnaire HealthQuestionnaire { get; private set; } = null!;

    public ClientFoodPreferences ClientFoodPreferences { get; private set; } = null!;
    
    public bool FoodWeighing { get; private set; }

    private Booking() { }

    private Booking(
        string serviceName,
        decimal servicePrice,
        int serviceId,
        string fullName,
        ClientMetrics clientMetrics,
        string purpose,
        HealthQuestionnaire healthQuestionnaire,
        ClientFoodPreferences clientFoodPreferences,
        bool foodWeighing)
    {
        ServiceName = serviceName;
        ServicePrice = servicePrice;
        ServiceId = serviceId;
        FullName = fullName;
        ClientMetrics = clientMetrics;
        Purpose = purpose;
        HealthQuestionnaire = healthQuestionnaire;
        ClientFoodPreferences = clientFoodPreferences;
        FoodWeighing = foodWeighing;
    }

    public static Result<Booking> Create(
        string serviceName,
        decimal servicePrice,
        int serviceId,
        string fullName,
        uint age,
        uint height,
        float weight,
        uint waistSize,
        string purpose,
        bool anxietyTendency,
        bool foodWeighing,
        CreateDiagnosisForBooking[]? diagnosesForBooking = null,
        string? feelingUnwellComplaints = null,
        string? allergies = null,
        string? intolerances = null,
        string? favoriteFoods = null,
        string? unfavoriteFoods = null,
        string? physicalActivities = null,
        string? stressAndHowYouCopeWithIt = null)
    {
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
            return new ErrorDetails("Айді послуши не може бути 0");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return new ErrorDetails("ПІБ не може бути пустим");
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            return new ErrorDetails("Мета схуднення не може бути пустою");
        }

        Result<ClientMetrics> createClientMetricsResult = ClientMetrics.Create(age, height, weight, waistSize);

        if (createClientMetricsResult.IsFailure)
        {
            return Result<Booking>.Failure(createClientMetricsResult);
        }

        Result<HealthQuestionnaire> createHealthQuestionnaireResult = HealthQuestionnaire.Create(
            anxietyTendency,
            feelingUnwellComplaints,
            allergies,
            intolerances,
            physicalActivities,
            stressAndHowYouCopeWithIt
        );

        if (createHealthQuestionnaireResult.IsFailure)
        {
            return Result<Booking>.Failure(createHealthQuestionnaireResult);
        }
        
        Result<ClientFoodPreferences> createClientFoodPreferencesResult = ClientFoodPreferences.Create(favoriteFoods, unfavoriteFoods);

        if (createClientFoodPreferencesResult.IsFailure)
        {
            return Result<Booking>.Failure(createClientFoodPreferencesResult);
        }
        
        Booking booking = new(
            serviceName,
            servicePrice,
            serviceId,
            fullName,
            createClientMetricsResult.Value!,
            purpose,
            createHealthQuestionnaireResult.Value!,
            createClientFoodPreferencesResult.Value!,
            foodWeighing
        );

        if (diagnosesForBooking is null)
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
            Result<Diagnosis> createDiagnosisForBooking = Diagnosis.Create(diagnosisForBooking.Name, diagnosisForBooking.MedicineNames);

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
