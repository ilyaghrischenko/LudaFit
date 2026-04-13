namespace LudaFit.SharedKernel.Options;

public sealed record DiagnosisOptions(string Name, IReadOnlyCollection<MedicineOptions> Medicines);
