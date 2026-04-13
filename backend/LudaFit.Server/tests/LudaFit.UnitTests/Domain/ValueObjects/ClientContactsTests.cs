using System.Net;
using FluentAssertions;
using LudaFit.Domain.ValueObjects;
using LudaFit.SharedKernel.Models;

namespace LudaFit.UnitTests.Domain.ValueObjects;

public sealed class ClientContactsTests
{
    [Theory]
    [InlineData("+380671112233", "ivan.petrenko@example.com", "+380671112233", "ivan.petrenko@example.com")]
    [InlineData("  +380671112233  ", "  ivan.petrenko@example.com  ", "+380671112233", "ivan.petrenko@example.com")]
    [InlineData("380671112233", "client@example.com", "380671112233", "client@example.com")]
    [InlineData("0671112233", "client@example.com", "0671112233", "client@example.com")]
    [InlineData("+447911123456", "uk.client@example.com", "+447911123456", "uk.client@example.com")]
    [InlineData("07911 123456", "uk.local@example.com", "07911 123456", "uk.local@example.com")]
    public void Create_ShouldReturnSuccess_WhenPhoneNumberAndEmailAreValid(
        string phoneNumber,
        string email,
        string expectedPhoneNumber,
        string expectedEmail)
    {
        // Arrange

        // Act
        Result<ClientContacts> result = ClientContacts.Create(phoneNumber, email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorDetails.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.PhoneNumber.Should().Be(expectedPhoneNumber);
        result.Value.Email.Should().NotBeNull();
        result.Value.Email.Address.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData("12345")]
    [InlineData("+0123456789")]
    [InlineData("invalid-phone")]
    public void Create_ShouldReturnFailure_WhenPhoneNumberIsInvalid(string phoneNumber)
    {
        // Arrange
        const string expectedMessage = "Номер телефону вказаний не вірно";

        // Act
        Result<ClientContacts> result = ClientContacts.Create(phoneNumber, "ivan.petrenko@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData("plainaddress")]
    [InlineData("ivan.petrenko@")]
    [InlineData("@example.com")]
    public void Create_ShouldReturnFailure_WhenEmailIsInvalid(string email)
    {
        // Arrange
        const string expectedMessage = "Пошта вказана не вірно";

        // Act
        Result<ClientContacts> result = ClientContacts.Create("+380671112233", email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Create_ShouldPrioritizePhoneNumberValidation_BeforeEmailValidation()
    {
        // Arrange
        const string expectedMessage = "Номер телефону вказаний не вірно";

        // Act
        Result<ClientContacts> result = ClientContacts.Create("invalid-phone", "invalid-email");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorDetails.Should().NotBeNull();
        result.ErrorDetails!.ErrorMessage.Should().Be(expectedMessage);
        result.ErrorDetails.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
