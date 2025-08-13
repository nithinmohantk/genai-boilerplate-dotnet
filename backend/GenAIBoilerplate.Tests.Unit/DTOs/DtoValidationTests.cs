using FluentAssertions;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Core.Enums;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GenAIBoilerplate.Tests.Unit.DTOs;

public class DtoValidationTests
{
    [Fact]
    public void LoginRequestDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "ValidPassword123"
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequestDto_WithInvalidEmail_ShouldFailValidation()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "invalid-email",
            Password = "ValidPassword123"
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterRequestDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "ValidPassword123",
            FirstName = "Test",
            LastName = "User",
            Username = "testuser"
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Password is required")]
    [InlineData("123", "Password must be at least 6 characters long")]
    public void RegisterRequestDto_WithInvalidPassword_ShouldFailValidation(string password, string expectedError)
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "Test",
            LastName = "User",
            Username = "testuser"
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().NotBeEmpty();
        var hasExpectedError = validationResults.Any(v => v.ErrorMessage != null && v.ErrorMessage.Contains("Password"));
        hasExpectedError.Should().BeTrue();
    }

    [Fact]
    public void TokenResponseDto_ShouldHaveCorrectProperties()
    {
        // Arrange
        var dto = new TokenResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token", 
            ExpiresIn = 3600,
            User = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" }
        };

        // Assert
        dto.AccessToken.Should().Be("access-token");
        dto.RefreshToken.Should().Be("refresh-token");
        dto.ExpiresIn.Should().Be(3600);
        dto.User.Should().NotBeNull();
        dto.User!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void UserDto_ShouldMapCorrectly()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            FullName = "Test User",
            IsActive = true,
            Role = UserRole.User
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Email.Should().Be("test@example.com");
        dto.Username.Should().Be("testuser");
        dto.FullName.Should().Be("Test User");
        dto.IsActive.Should().BeTrue();
        dto.Role.Should().Be(UserRole.User);
    }

    private static List<ValidationResult> ValidateDto(object dto)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
        Validator.TryValidateObject(dto, validationContext, validationResults, true);
        return validationResults;
    }
}
