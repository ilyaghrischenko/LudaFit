namespace LudaFit.Infrastructure.Gmail.Options;

public sealed record BookingOptions(
    string ServiceName,
    decimal ServicePrice,
    uint Age,
    uint Height,
    float Weight,
    uint WaistSize,
    string Purpose,
    string? PhysicalActivities,
    ClientAdditionalInformationOptions? ClientAdditionalInformation,
    IReadOnlyCollection<DiagnosisOptions> Diagnoses
);
    
