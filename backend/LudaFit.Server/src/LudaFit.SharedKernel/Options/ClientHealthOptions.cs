namespace LudaFit.SharedKernel.Options;

public sealed record ClientHealthOptions(
    string? FeelingUnwellComplaints,
    string? Allergies,
    string? Intolerances,
    string? StressAndHowYouCopeWithIt,
    bool? AnxietyTendency
);
