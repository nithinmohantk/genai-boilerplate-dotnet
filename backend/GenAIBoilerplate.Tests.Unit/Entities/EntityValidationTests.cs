using FluentAssertions;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Enums;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GenAIBoilerplate.Tests.Unit.Entities;

public class EntityValidationTests
{
    [Fact]
    public void User_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            FullName = "Test User",
            HashedPassword = "hashed_password_here",
            Role = UserRole.User,
            TenantId = Guid.NewGuid(),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(user);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void User_WithInvalidEmail_ShouldFailValidation()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "invalid-email", // Invalid email format
            Username = "testuser",
            FullName = "Test User",
            HashedPassword = "hashed_password_here",
            Role = UserRole.User,
            TenantId = Guid.NewGuid(),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(user);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.ErrorMessage!.Contains("valid e-mail"));
    }

    [Fact]
    public void User_WithEmptyRequiredFields_ShouldFailValidation()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "", // Required field empty
            Username = "", // Required field empty
            FullName = "", // Required field empty
            HashedPassword = "",
            Role = UserRole.User,
            TenantId = Guid.NewGuid(),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(user);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Count().Should().BeGreaterThanOrEqualTo(1); // At least 1 validation error
    }

    [Fact]
    public void Tenant_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            Domain = "test.example.com",
            Description = "A test tenant for validation",
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(tenant);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Tenant_WithEmptyRequiredFields_ShouldFailValidation()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "", // Required field empty
            Domain = "", // Required field empty
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(tenant);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Count().Should().BeGreaterThanOrEqualTo(2); // At least 2 validation errors
    }

    [Fact]
    public void Tenant_WithTooLongFields_ShouldFailValidation()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = new string('x', 101), // Exceeds 100 character limit
            Domain = new string('y', 101), // Exceeds 100 character limit
            Description = new string('z', 501), // Exceeds 500 character limit
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(tenant);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Count().Should().BeGreaterThanOrEqualTo(3); // At least 3 validation errors
    }

    [Fact]
    public void ChatSession_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var chatSession = new ChatSession
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Title = "Test Chat Session",
            Model = "gpt-4",
            SystemPrompt = "You are a helpful assistant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(chatSession);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ChatMessage_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Content = "Hello, this is a test message.",
            Role = MessageRole.User,
            ModelName = "gpt-4",
            TokenCount = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(chatMessage);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ChatMessage_WithEmptyContent_ShouldFailValidation()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Content = "", // Required field empty
            Role = MessageRole.User,
            ModelName = "gpt-4",
            TokenCount = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(chatMessage);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains(nameof(ChatMessage.Content)));
    }

    [Fact]
    public void TenantApiKey_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var apiKey = new TenantApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test API Key",
            KeyHash = "hashed_key_here",
            KeyPrefix = "sk-test",
            Provider = "openai",
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(apiKey);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void RefreshToken_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hashed_token_here",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(refreshToken);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void RefreshToken_WithEmptyTokenHash_ShouldFailValidation()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "", // Required field empty
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(refreshToken);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains(nameof(RefreshToken.TokenHash)));
    }

    [Theory]
    [InlineData(UserRole.SuperAdmin)]
    [InlineData(UserRole.TenantAdmin)]
    [InlineData(UserRole.TenantUser)]
    [InlineData(UserRole.User)]
    public void UserRole_AllValidValues_ShouldBeAccepted(UserRole role)
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            FullName = "Test User",
            HashedPassword = "hashed_password_here",
            Role = role, // Test different role values
            TenantId = Guid.NewGuid(),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(user);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(TenantStatus.Active)]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Pending)]
    public void TenantStatus_AllValidValues_ShouldBeAccepted(TenantStatus status)
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            Domain = "test.example.com",
            Description = "A test tenant for validation",
            Status = status, // Test different status values
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateEntity(tenant);

        // Assert
        validationResults.Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateEntity(object entity)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(entity);
        Validator.TryValidateObject(entity, validationContext, validationResults, true);
        return validationResults;
    }
}
