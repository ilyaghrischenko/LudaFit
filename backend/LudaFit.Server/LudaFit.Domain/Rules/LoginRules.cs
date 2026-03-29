using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Rules;

public static class LoginRules
{
    public static bool MustNotBeNullOrWhiteSpace(string login)
        => !string.IsNullOrWhiteSpace(login);

    public static Result IsValid(string login)
    {
        if (MustNotBeNullOrWhiteSpace(login) is false)
        {
            return new ErrorDetails("Логін не може бути пустим");
        }
        
        return Result.Success();
    }
}