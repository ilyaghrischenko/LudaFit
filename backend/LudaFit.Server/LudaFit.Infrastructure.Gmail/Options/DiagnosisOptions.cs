namespace LudaFit.Infrastructure.Gmail.Options;

public sealed record DiagnosisOptions(string Name, IReadOnlyCollection<MedicineOptions> Medicines);
