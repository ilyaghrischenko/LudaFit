using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Rules;

public static class PasswordRules
{
    public static bool MustNotBeNullOrWhiteSpace(string password)
        => !string.IsNullOrWhiteSpace(password);

    public static bool MustBeAtLeast8CharactersLong(string password)
        => password.Length >= 8;
    
    public static bool MustContainAtLeastOneUppercaseLetter(string password)
        => password.Any(char.IsUpper);

    public static bool MustContainAtLeastOneLowercaseLetter(string password)
        => password.Any(char.IsLower);
    
    public static bool MustContainAtLeastOneDigit(string password)
        => password.Any(char.IsDigit);
    
    public static bool MustContainAtLeastOneSpecialCharacter(string password)
        => password.Any(char.IsPunctuation);

    public static Result IsValid(string password)
    {
        if (MustNotBeNullOrWhiteSpace(password))
        {
            return new ErrorDetails("Пароль не може бути пустим");
        }

        if (MustBeAtLeast8CharactersLong(password))
        {
            return new ErrorDetails("Пароль повинен мати мінімум 8 символів");
        }

        if (MustContainAtLeastOneUppercaseLetter(password))
        {
            return new ErrorDetails("Пароль повинен мати хоча б одну велику літеру");
        }

        if (MustContainAtLeastOneLowercaseLetter(password))
        {
            return new ErrorDetails("Пароль повинен мати хоча б одну малу літеру");
        }

        if (MustContainAtLeastOneDigit(password))
        {
            return new ErrorDetails("Пароль повинен мати хоча б одну цифру");
        }

        if (MustContainAtLeastOneSpecialCharacter(password))
        {
            return new ErrorDetails("Пароль повинен мати хоча б один спеціальний символ");
        }
        
        return Result.Success();
    }
}