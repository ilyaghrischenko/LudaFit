using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.ValueObjects;

public sealed record HealthQuestionnaire
{
    public string? FeelingUnwellComplaints { get; init; }
    
    public string? Allergies { get; init; }
    
    public string? Intolerances { get; init; }
    
    public string? PhysicalActivities { get; init; }
    
    public string? StressAndHowYouCopeWithIt { get; init; }
    
    public bool AnxietyTendency { get; init; }

    private HealthQuestionnaire() { }

    private HealthQuestionnaire(
        string? feelingUnwellComplaints,
        string? allergies,
        string? intolerances,
        string? physicalActivities,
        string? stressAndHowYouCopeWithIt,
        bool anxietyTendency)
    {
        FeelingUnwellComplaints = feelingUnwellComplaints;
        Allergies = allergies;
        Intolerances = intolerances;
        PhysicalActivities = physicalActivities;
        StressAndHowYouCopeWithIt = stressAndHowYouCopeWithIt;
        AnxietyTendency = anxietyTendency;
    }

    public static Result<HealthQuestionnaire> Create(
        bool anxietyTendency,
        string? feelingUnwellComplaints = null,
        string? allergies = null,
        string? intolerances = null,
        string? physicalActivities = null,
        string? stressAndHowYouCopeWithIt = null)
    {
        if (feelingUnwellComplaints is not null && string.IsNullOrWhiteSpace(feelingUnwellComplaints))
        {
            return new ErrorDetails("Скарги на самопочуття не можуть бути пустими");
        }
        
        if (allergies is not null && string.IsNullOrWhiteSpace(allergies))
        {
            return new ErrorDetails("Алергії не можуть бути пустими");
        }
        
        if (intolerances is not null && string.IsNullOrWhiteSpace(intolerances))
        {
            return new ErrorDetails("Не переносимість їжі не може бути пустим");
        }
        
        if (physicalActivities is not null && string.IsNullOrWhiteSpace(physicalActivities))
        {
            return new ErrorDetails("Фізична активність не може бути пустою");
        }
        
        if (stressAndHowYouCopeWithIt is not null && string.IsNullOrWhiteSpace(stressAndHowYouCopeWithIt))
        {
            return new ErrorDetails("Як справляєтесь зі стресом не може бути пустим полем");
        }

        return new HealthQuestionnaire(
            feelingUnwellComplaints,
            allergies,
            intolerances,
            physicalActivities,
            stressAndHowYouCopeWithIt,
            anxietyTendency
        );
    }
}
