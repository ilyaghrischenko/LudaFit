using System.Text.RegularExpressions;

namespace LudaFit.Domain.Rules;

public static partial class PhoneNumberRules
{
    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex InternationalE164Regex();

    [GeneratedRegex(@"^(?:\+380|380|0)\s?\(?\d{2}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$")]
    private static partial Regex UkraineRegex();

    [GeneratedRegex(@"^(?:\+44|0)\s?\(?\d{2,5}\)?[\s\-]?\d{3,4}[\s\-]?\d{3,4}$")]
    private static partial Regex UkRegex();
    
    public static bool IsValid(string phoneNumber)
        => !string.IsNullOrWhiteSpace(phoneNumber)
            && (InternationalE164Regex().IsMatch(phoneNumber)
            || UkraineRegex().IsMatch(phoneNumber)
            || UkRegex().IsMatch(phoneNumber));
}
