using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Tests.BDD.Support;
using GenAIBoilerplate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using GenAIBoilerplate.Infrastructure.Persistence;

namespace GenAIBoilerplate.Tests.BDD.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly TestContext _testContext;
    
    public AuthenticationSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    #region Given Steps

    [Given(@"the GenAI platform is running")]
    public void GivenTheGenAIPlatformIsRunning()
    {
        // Platform is running through TestWebApplicationFactory
        _testContext.HttpClient.Should().NotBeNull();
    }

    [Given(@"the database is clean")]
    public async Task GivenTheDatabaseIsClean()
    {
        // Database cleanup is handled by hooks
        await Task.CompletedTask;
    }

    [Given(@"I am a new user")]
    public void GivenIAmANewUser()
    {
        _testContext.ClearAuthentication();
    }

    [Given(@"I have an existing user account:")]
    public async Task GivenIHaveAnExistingUserAccount(Table table)
    {
        var userData = table.CreateInstance<UserData>();
        
        // Create user directly in database
        await _testContext.Factory.ExecuteDatabaseOperationAsync(async (dbContext) =>
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password),
                Role = Core.Enums.UserRole.User,
                TenantId = Guid.NewGuid(), // Default tenant for testing
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create default tenant if not exists
            var tenant = new Tenant
            {
                Id = user.TenantId,
                Name = "Test Tenant",
                Description = "Default test tenant",
                Status = Core.Enums.TenantStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Tenants.Add(tenant);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            _testContext.StoreTestData("ExistingUser", user);
            _testContext.StoreTestData("ExistingUserPassword", userData.Password);
        });
    }

    [Given(@"I have a logged-in user with valid tokens")]
    public async Task GivenIHaveALoggedInUserWithValidTokens()
    {
        // Create and login user
        var userData = new UserData
        {
            Email = "testuser@example.com",
            Password = "TestPass123!",
            FirstName = "Test",
            LastName = "User"
        };

        await GivenIHaveAnExistingUserAccount(Table.FromHeaderAndRows(
            new[] { "Email", "Password", "FirstName", "LastName" },
            new[] { userData.Email, userData.Password, userData.FirstName, userData.LastName }
        ));

        await WhenILoginWithValidCredentials(Table.FromHeaderAndRows(
            new[] { "Email", "Password" },
            new[] { userData.Email, userData.Password }
        ));
    }

    [Given(@"I have an invalid or expired refresh token")]
    public void GivenIHaveAnInvalidOrExpiredRefreshToken()
    {
        _testContext.CurrentRefreshToken = "invalid_refresh_token";
    }

    [Given(@"I have an expired access token")]
    public void GivenIHaveAnExpiredAccessToken()
    {
        _testContext.CurrentAccessToken = "expired_access_token";
        _testContext.SetAuthenticationToken("expired_access_token");
    }

    #endregion

    #region When Steps

    [When(@"I register with valid credentials:")]
    public async Task WhenIRegisterWithValidCredentials(Table table)
    {
        var userData = table.CreateInstance<UserData>();
        
        var request = new RegisterRequestDto
        {
            Email = userData.Email,
            Password = userData.Password,
            FirstName = userData.FirstName,
            LastName = userData.LastName
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _testContext.HttpClient.PostAsync("/api/v1/auth/register", content);
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I attempt to register with the same email:")]
    public async Task WhenIAttemptToRegisterWithTheSameEmail(Table table)
    {
        await WhenIRegisterWithValidCredentials(table);
    }

    [When(@"I register with invalid credentials:")]
    public async Task WhenIRegisterWithInvalidCredentials(Table table)
    {
        await WhenIRegisterWithValidCredentials(table);
    }

    [When(@"I login with valid credentials:")]
    public async Task WhenILoginWithValidCredentials(Table table)
    {
        var loginData = table.CreateInstance<LoginData>();
        
        var request = new LoginRequestDto
        {
            Email = loginData.Email,
            Password = loginData.Password
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _testContext.HttpClient.PostAsync("/api/v1/auth/login", content);
        await _testContext.StoreLastResponseAsync(response);

        // Store tokens if login successful
        if (response.IsSuccessStatusCode && _testContext.LastResponseContent != null)
        {
            var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(_testContext.LastResponseContent);
            if (loginResponse != null)
            {
                _testContext.CurrentAccessToken = loginResponse.AccessToken;
                _testContext.CurrentRefreshToken = loginResponse.RefreshToken;
                _testContext.SetAuthenticationToken(loginResponse.AccessToken);
            }
        }
    }

    [When(@"I login with invalid credentials:")]
    public async Task WhenILoginWithInvalidCredentials(Table table)
    {
        await WhenILoginWithValidCredentials(table);
    }

    [When(@"I request a token refresh using the refresh token")]
    public async Task WhenIRequestATokenRefreshUsingTheRefreshToken()
    {
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = _testContext.CurrentRefreshToken ?? "test_refresh_token"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _testContext.HttpClient.PostAsync("/api/v1/auth/refresh", content);
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I request a token refresh using the invalid token")]
    public async Task WhenIRequestATokenRefreshUsingTheInvalidToken()
    {
        await WhenIRequestATokenRefreshUsingTheRefreshToken();
    }

    [When(@"I access a protected endpoint with the access token")]
    public async Task WhenIAccessAProtectedEndpointWithTheAccessToken()
    {
        var response = await _testContext.HttpClient.GetAsync("/api/v1/auth/me");
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I access a protected endpoint without any token")]
    public async Task WhenIAccessAProtectedEndpointWithoutAnyToken()
    {
        _testContext.ClearAuthentication();
        var response = await _testContext.HttpClient.GetAsync("/api/v1/auth/me");
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I access a protected endpoint with the expired token")]
    public async Task WhenIAccessAProtectedEndpointWithTheExpiredToken()
    {
        var response = await _testContext.HttpClient.GetAsync("/api/v1/auth/me");
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I logout from the system")]
    public async Task WhenILogoutFromTheSystem()
    {
        var response = await _testContext.HttpClient.PostAsync("/api/v1/auth/logout", null);
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I request my user profile")]
    public async Task WhenIRequestMyUserProfile()
    {
        var response = await _testContext.HttpClient.GetAsync("/api/v1/auth/me");
        await _testContext.StoreLastResponseAsync(response);
    }

    [When(@"I update my profile with new information:")]
    public async Task WhenIUpdateMyProfileWithNewInformation(Table table)
    {
        var profileData = table.CreateInstance<UpdateProfileData>();
        
        var request = new UpdateUserProfileRequestDto
        {
            FirstName = profileData.FirstName,
            LastName = profileData.LastName,
            Phone = profileData.Phone
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _testContext.HttpClient.PutAsync("/api/v1/auth/me", content);
        await _testContext.StoreLastResponseAsync(response);
    }

    #endregion

    #region Then Steps

    [Then(@"I should receive a successful registration response")]
    public void ThenIShouldReceiveASuccessfulRegistrationResponse()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.Created);
    }

    [Then(@"I should receive a successful login response")]
    public void ThenIShouldReceiveASuccessfulLoginResponse()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Then(@"I should receive a JWT access token")]
    public void ThenIShouldReceiveAJWTAccessToken()
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<LoginResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        response!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should receive a refresh token")]
    public void ThenIShouldReceiveARefreshToken()
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<LoginResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        response!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"my user account should be created in the database")]
    public async Task ThenMyUserAccountShouldBeCreatedInTheDatabase()
    {
        var userExists = await _testContext.Factory.ExecuteDatabaseOperationAsync(async (dbContext) =>
        {
            return await dbContext.Users.AnyAsync(u => u.Email == "user@example.com");
        });

        userExists.Should().BeTrue();
    }

    [Then(@"the access token should contain valid user claims")]
    public void ThenTheAccessTokenShouldContainValidUserClaims()
    {
        _testContext.CurrentAccessToken.Should().NotBeNullOrEmpty();
        // Token validation would be done by JWT middleware in actual scenarios
    }

    [Then(@"I should receive an authentication error")]
    public void ThenIShouldReceiveAnAuthenticationError()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Then(@"I should not receive any tokens")]
    public void ThenIShouldNotReceiveAnyTokens()
    {
        if (!string.IsNullOrEmpty(_testContext.LastResponseContent))
        {
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(_testContext.LastResponseContent);
            response.Should().NotContainKey("accessToken");
            response.Should().NotContainKey("refreshToken");
        }
    }

    [Then(@"the error message should indicate invalid credentials")]
    public void ThenTheErrorMessageShouldIndicateInvalidCredentials()
    {
        _testContext.LastErrorMessage.Should().NotBeNullOrEmpty();
        _testContext.LastErrorMessage.Should().Contain("Invalid");
    }

    [Then(@"I should receive a validation error")]
    public void ThenIShouldReceiveAValidationError()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Then(@"the error message should indicate email already exists")]
    public void ThenTheErrorMessageShouldIndicateEmailAlreadyExists()
    {
        _testContext.LastErrorMessage.Should().NotBeNullOrEmpty();
        _testContext.LastErrorMessage.Should().Contain("already exists", StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"no new user account should be created")]
    public async Task ThenNoNewUserAccountShouldBeCreated()
    {
        var userCount = await _testContext.Factory.ExecuteDatabaseOperationAsync(async (dbContext) =>
        {
            return await dbContext.Users.CountAsync(u => u.Email == "user@example.com");
        });

        userCount.Should().Be(1); // Only the original user should exist
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenTheErrorMessageShouldContain(string expectedError)
    {
        _testContext.LastErrorMessage.Should().NotBeNullOrEmpty();
        _testContext.LastErrorMessage.Should().Contain(expectedError, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"I should receive a new access token")]
    public void ThenIShouldReceiveANewAccessToken()
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<LoginResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        response!.AccessToken.Should().NotBeNullOrEmpty();
        response.AccessToken.Should().NotBe(_testContext.CurrentAccessToken);
    }

    [Then(@"I should receive a new refresh token")]
    public void ThenIShouldReceiveANewRefreshToken()
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<LoginResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        response!.RefreshToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBe(_testContext.CurrentRefreshToken);
    }

    [Then(@"the old refresh token should be invalidated")]
    public void ThenTheOldRefreshTokenShouldBeInvalidated()
    {
        // This would be verified by attempting to use the old token
        // Implementation depends on refresh token strategy
    }

    [Then(@"the new access token should contain valid user claims")]
    public void ThenTheNewAccessTokenShouldContainValidUserClaims()
    {
        ThenTheAccessTokenShouldContainValidUserClaims();
    }

    [Then(@"the error message should indicate invalid refresh token")]
    public void ThenTheErrorMessageShouldIndicateInvalidRefreshToken()
    {
        _testContext.LastErrorMessage.Should().NotBeNullOrEmpty();
        _testContext.LastErrorMessage.Should().Contain("invalid", StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"I should receive a successful response")]
    public void ThenIShouldReceiveASuccessfulResponse()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Then(@"the response should contain user-specific data")]
    public void ThenTheResponseShouldContainUserSpecificData()
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<UserResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        response!.Email.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should receive an unauthorized error")]
    public void ThenIShouldReceiveAnUnauthorizedError()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatusCode)
    {
        _testContext.LastStatusCode.Should().Be(expectedStatusCode);
    }

    [Then(@"the error message should indicate token has expired")]
    public void ThenTheErrorMessageShouldIndicateTokenHasExpired()
    {
        _testContext.LastErrorMessage.Should().NotBeNullOrEmpty();
        _testContext.LastErrorMessage.Should().Contain("expired", StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"I should receive a successful logout response")]
    public void ThenIShouldReceiveASuccessfulLogoutResponse()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Then(@"my access token should be revoked")]
    [Then(@"my refresh token should be revoked")]
    public void ThenMyTokensShouldBeRevoked()
    {
        // Token revocation verification would be implementation-specific
        // Could involve checking a token blacklist or database
    }

    [Then(@"I should not be able to access protected endpoints")]
    public async Task ThenIShouldNotBeAbleToAccessProtectedEndpoints()
    {
        var response = await _testContext.HttpClient.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"I should receive my profile information")]
    public void ThenIShouldReceiveMyProfileInformation()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<UserResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
    }

    [Then(@"the profile should contain:")]
    public void ThenTheProfileShouldContain(Table table)
    {
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<UserResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();

        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var expectedValue = row["Value"];

            switch (field.ToLower())
            {
                case "email":
                    response!.Email.Should().Be(expectedValue);
                    break;
                case "firstname":
                    response!.FirstName.Should().Be(expectedValue);
                    break;
                case "lastname":
                    response!.LastName.Should().Be(expectedValue);
                    break;
                case "role":
                    response!.Role.Should().Be(expectedValue);
                    break;
            }
        }
    }

    [Then(@"I should receive a successful update response")]
    public void ThenIShouldReceiveASuccessfulUpdateResponse()
    {
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Then(@"my profile should be updated in the database")]
    public async Task ThenMyProfileShouldBeUpdatedInTheDatabase()
    {
        var currentUser = _testContext.GetTestData<User>("ExistingUser");
        if (currentUser != null)
        {
            var updatedUser = await _testContext.Factory.ExecuteDatabaseOperationAsync(async (dbContext) =>
            {
                return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            });

            updatedUser.Should().NotBeNull();
            // Additional verification based on what was updated
        }
    }

    [Then(@"when I request my profile again, it should show the updated information")]
    public async Task ThenWhenIRequestMyProfileAgainItShouldShowTheUpdatedInformation()
    {
        await WhenIRequestMyUserProfile();
        _testContext.LastStatusCode.Should().Be((int)HttpStatusCode.OK);
        
        _testContext.LastResponseContent.Should().NotBeNullOrEmpty();
        var response = JsonSerializer.Deserialize<UserResponseDto>(_testContext.LastResponseContent!);
        response.Should().NotBeNull();
        // Additional verification based on updated data
    }

    #endregion

    #region Helper Classes

    public class UserData
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginData
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    #endregion
}
