using LudaFit.Domain.Entities.Common;
using LudaFit.Domain.Rules;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Admin : BaseEntity
{
    public string Login { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    private Admin() { }

    private Admin(string login, string passwordHash)
    {
        Login = login;
        PasswordHash = passwordHash;
    }

    public static Result<Admin> Create(string login, string password, string passwordHash)
    {
        Result validationResult = LoginRules.IsValid(login);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }

        validationResult = PasswordRules.IsValid(password);

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

    public Result ChangePassword(string oldPasswordHash, string newPassword, string newPasswordHash)
    {
        if (oldPasswordHash == newPasswordHash)
        {
            return new ErrorDetails("Новий пароль має відрізнятися");
        }
        
        Result validationResult = PasswordRules.IsValid(newPassword);

        if (validationResult.IsFailure)
        {
            return new ErrorDetails(validationResult.ErrorDetails!.ErrorMessage);
        }
        
        PasswordHash = newPasswordHash;
        return Result.Success();
    }
}
