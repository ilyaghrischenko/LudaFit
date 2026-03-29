using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.Rules;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Admin : BaseEntity
{
    public string Login { get; private set; }
    
    public string PasswordHash { get; private set; }

    private Admin()
    {
        Login = null!;
        PasswordHash = null!;
    }

    private Admin(string login, string passwordHash)
    {
        Login = login;
        PasswordHash = passwordHash;
    }

    public static Result<Admin> Create(string login, string passwordHash)
    {
        Result validationResult = LoginRules.IsValid(login);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }

        //todo: может быть проблема так как это хэш а не сам пароль, может не пройти по требованиям
        validationResult = PasswordRules.IsValid(passwordHash);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }

        return new Admin(
            login,
            passwordHash
        );
    }

    public Result ChangeLogin(string newLogin)
    {
        if (Login == newLogin)
        {
            return new ErrorDetails("Новий логін має відрізнятися");
        }
        
        Result validationResult = LoginRules.IsValid(newLogin);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }
        
        Login = newLogin;
        return Result.Success();
    }

    public Result ChangePassword(string oldPasswordHash, string newPasswordHash)
    {
        if (oldPasswordHash == newPasswordHash)
        {
            return new ErrorDetails("Новий пароль має відрізнятися");
        }
        
        //todo: может быть проблема так как это хэш а не сам пароль, может не пройти по требованиям
        Result validationResult = PasswordRules.IsValid(newPasswordHash);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }
        
        PasswordHash = newPasswordHash;
        return Result.Success();
    }
}