using FluentAssertions;
using LudaFit.Domain.Entities;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.Entities;

public sealed class AdminTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenLoginAndPasswordHashAreValid()
    {
        // Arrange
        const string login = "admin";
        const string passwordHash = "Password1!";

        // Act
        Result<Admin> result = Admin.Create(login, passwordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Login.Should().Be(login);
        result.Value.PasswordHash.Should().Be(passwordHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenLoginIsNullOrWhiteSpace(string? login)
    {
        // Arrange
        const string passwordHash = "Password1!";
        const string expectedMessage = "Логін не може бути пустим";

        // Act
        Result<Admin> result = Admin.Create(login!, passwordHash);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Theory]
    [InlineData(null, "Пароль не може бути пустим")]
    [InlineData("", "Пароль не може бути пустим")]
    [InlineData(" ", "Пароль не може бути пустим")]
    [InlineData("   ", "Пароль не може бути пустим")]
    [InlineData("Short1!", "Пароль повинен мати мінімум 8 символів")]
    [InlineData("password1!", "Пароль повинен мати хоча б одну велику літеру")]
    [InlineData("PASSWORD1!", "Пароль повинен мати хоча б одну малу літеру")]
    [InlineData("Password!", "Пароль повинен мати хоча б одну цифру")]
    [InlineData("Password1", "Пароль повинен мати хоча б один спеціальний символ")]
    public void Create_ShouldReturnFailure_WhenPasswordHashIsInvalid(
        string? passwordHash,
        string expectedMessage)
    {
        // Arrange
        const string login = "admin";

        // Act
        Result<Admin> result = Admin.Create(login, passwordHash!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldPrioritizeLoginValidation_BeforePasswordHashValidation()
    {
        // Arrange
        string login = string.Empty;
        const string passwordHash = "short";
        const string expectedMessage = "Логін не може бути пустим";

        // Act
        Result<Admin> result = Admin.Create(login, passwordHash);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void ChangeLogin_ShouldReturnSuccess_WhenNewLoginIsValidAndDifferent()
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string newLogin = "new-admin";

        // Act
        Result result = admin.ChangeLogin(newLogin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        admin.Login.Should().Be(newLogin);
    }

    [Fact]
    public void ChangeLogin_ShouldReturnFailure_WhenNewLoginMatchesCurrentLogin()
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string currentLogin = "admin";
        const string expectedMessage = "Новий логін має відрізнятися";

        // Act
        Result result = admin.ChangeLogin(currentLogin);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        admin.Login.Should().Be(currentLogin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeLogin_ShouldReturnFailure_WhenNewLoginIsNullOrWhiteSpace(string? newLogin)
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string currentLogin = "admin";
        const string expectedMessage = "Логін не може бути пустим";

        // Act
        Result result = admin.ChangeLogin(newLogin!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        admin.Login.Should().Be(currentLogin);
    }

    [Fact]
    public void ChangePassword_ShouldReturnSuccess_WhenNewPasswordHashIsValidAndDifferent()
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string oldPasswordHash = "old-password-hash";
        const string newPasswordHash = "NewPassword1!";

        // Act
        Result result = admin.ChangePassword(oldPasswordHash, newPasswordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        admin.PasswordHash.Should().Be(newPasswordHash);
    }

    [Theory]
    [InlineData("Password1!")]
    [InlineData("same-password-hash")]
    public void ChangePassword_ShouldReturnFailure_WhenNewPasswordHashMatchesOldPasswordHash(string passwordHash)
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string initialPasswordHash = "Password1!";
        const string expectedMessage = "Новий пароль має відрізнятися";

        // Act
        Result result = admin.ChangePassword(passwordHash, passwordHash);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        admin.PasswordHash.Should().Be(initialPasswordHash);
    }

    [Theory]
    [InlineData(null, "Пароль не може бути пустим")]
    [InlineData("", "Пароль не може бути пустим")]
    [InlineData(" ", "Пароль не може бути пустим")]
    [InlineData("   ", "Пароль не може бути пустим")]
    [InlineData("Short1!", "Пароль повинен мати мінімум 8 символів")]
    [InlineData("password1!", "Пароль повинен мати хоча б одну велику літеру")]
    [InlineData("PASSWORD1!", "Пароль повинен мати хоча б одну малу літеру")]
    [InlineData("Password!", "Пароль повинен мати хоча б одну цифру")]
    [InlineData("Password1", "Пароль повинен мати хоча б один спеціальний символ")]
    public void ChangePassword_ShouldReturnFailure_WhenNewPasswordHashIsInvalid(
        string? newPasswordHash,
        string expectedMessage)
    {
        // Arrange
        Result<Admin> createResult = Admin.Create("admin", "Password1!");
        Admin admin = createResult.Value!;
        const string oldPasswordHash = "old-password-hash";
        const string initialPasswordHash = "Password1!";

        // Act
        Result result = admin.ChangePassword(oldPasswordHash, newPasswordHash!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        admin.PasswordHash.Should().Be(initialPasswordHash);
    }
}
