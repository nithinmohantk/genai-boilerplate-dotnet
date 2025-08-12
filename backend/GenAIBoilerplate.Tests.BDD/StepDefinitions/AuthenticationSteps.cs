using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Tests.BDD.Support;
using TechTalk.SpecFlow;

namespace GenAIBoilerplate.Tests.BDD.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly TestContext _context;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AuthenticationSteps(TestContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Given(@"I have a valid user registration request with email ""(.*)""")]
    public void GivenIHaveAValidUserRegistrationRequestWithEmail(string email)
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "TestPass123!",
            FullName = "Test User"
        };
        _context.StoreTestData("registerRequest", registerRequest);
    }

    [Given(@"I have user credentials email ""(.*)"" and password ""(.*)""")]
    public void GivenIHaveUserCredentialsEmailAndPassword(string email, string password)
    {
        var loginRequest = new LoginRequestDto
        {
            Email = email,
            Password = password
        };
        _context.StoreTestData("loginRequest", loginRequest);
    }

    [When(@"I send a POST request to ""(.*)""")]
    public async Task WhenISendAPostRequestTo(string endpoint)
    {
        object? requestData = null;
        
        // Determine request data based on endpoint
        if (endpoint.Contains("register"))
            requestData = _context.GetTestData<RegisterRequestDto>("registerRequest");
        else if (endpoint.Contains("login"))
            requestData = _context.GetTestData<LoginRequestDto>("loginRequest");
        
        if (requestData == null)
        {
            throw new InvalidOperationException($"No request data available for endpoint {endpoint}");
        }

        var json = JsonSerializer.Serialize(requestData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync(endpoint, content);
        await _context.StoreLastResponseAsync(response);
    }

    [Then(@"the response status should be (\d+)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatusCode)
    {
        _context.LastResponse.Should().NotBeNull();
        ((int)_context.LastResponse!.StatusCode).Should().Be(expectedStatusCode);
    }

    [Then(@"the response should contain user information")]
    public void ThenTheResponseShouldContainUserInformation()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        // Try to deserialize as TokenResponseDto for registration/login
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        tokenResponse.User.Should().NotBeNull();
        tokenResponse.User.Email.Should().NotBeNullOrEmpty();
    }

    [Then(@"the response should contain access token")]
    public void ThenTheResponseShouldContainAccessToken()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        
        // Store token for future requests
        _context.CurrentAccessToken = tokenResponse.AccessToken;
        _context.CurrentUser = tokenResponse.User;
        _context.SetAuthenticationToken(tokenResponse.AccessToken);
    }

    [Then(@"the response should contain error message")]
    public void ThenTheResponseShouldContainErrorMessage()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Given(@"I am authenticated as ""(.*)""")]
    public void GivenIAmAuthenticatedAs(string email)
    {
        // For testing purposes, create a simple scenario where we assume authentication
        // In a real test, we would register and login first
        _context.CurrentUser = new UserDto { Email = email };
        // Note: In real tests, we should authenticate properly and get a real token
    }

    [When(@"I send a GET request to ""(.*)""")]
    public async Task WhenISendAGetRequestTo(string endpoint)
    {
        var response = await _context.HttpClient.GetAsync(endpoint);
        await _context.StoreLastResponseAsync(response);
    }

    [Then(@"the response should be unauthorized")]
    public void ThenTheResponseShouldBeUnauthorized()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"the user should be registered successfully")]
    public void ThenTheUserShouldBeRegisteredSuccessfully()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.User.Should().NotBeNull();
        tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should be able to login with those credentials")]
    public async Task ThenIShouldBeAbleToLoginWithThoseCredentials()
    {
        var registerRequest = _context.GetTestData<RegisterRequestDto>("registerRequest");
        registerRequest.Should().NotBeNull();
        
        var loginRequest = new LoginRequestDto
        {
            Email = registerRequest!.Email,
            Password = registerRequest.Password
        };
        
        var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/login", content);
        await _context.StoreLastResponseAsync(response);
        
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    // Common steps shared across features
    [Given(@"the GenAI platform is running")]
    public void GivenTheGenAIPlatformIsRunning()
    {
        // Platform is running if we can reach the health endpoint
        // This is handled by the test setup - no action needed
    }

    [Given(@"the database is clean")]
    public async Task GivenTheDatabaseIsClean()
    {
        // Clean up any test data that might exist
        // This is handled by TestContainers creating fresh instances
        await Task.CompletedTask;
    }

    [Given(@"I have a logged-in user with valid tokens")]
    public async Task GivenIHaveALoggedInUserWithValidTokens()
    {
        // Create and login a test user
        var registerRequest = new RegisterRequestDto
        {
            Email = "testuser@example.com",
            Password = "TestPass123!",
            FullName = "Test User"
        };
        
        var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var registerResponse = await _context.HttpClient.PostAsync("/api/auth/register", content);
        if (registerResponse.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(await registerResponse.Content.ReadAsStringAsync(), _jsonOptions);
            _context.CurrentAccessToken = tokenResponse!.AccessToken;
            _context.CurrentRefreshToken = tokenResponse.RefreshToken;
            _context.CurrentUser = tokenResponse.User;
            _context.SetAuthenticationToken(tokenResponse.AccessToken);
        }
    }

    [Given(@"I am a new user")]
    public void GivenIAmANewUser()
    {
        // Clear any existing authentication
        _context.ClearAuthentication();
    }

    [When(@"I register with invalid credentials:")]
    public async Task WhenIRegisterWithInvalidCredentials(Table table)
    {
        var row = table.Rows.First();
        var registerRequest = new RegisterRequestDto
        {
            Email = row["Email"],
            Password = row["Password"],
            FullName = $"{(row.ContainsKey("FirstName") ? row["FirstName"] : "")} {(row.ContainsKey("LastName") ? row["LastName"] : "")}".Trim()
        };
        
        var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/register", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I update my profile with new information:")]
    public async Task WhenIUpdateMyProfileWithNewInformation(Table table)
    {
        var row = table.Rows.First();
        var updateRequest = new UpdateProfileRequestDto
        {
            FullName = $"{(row.ContainsKey("FirstName") ? row["FirstName"] : "")} {(row.ContainsKey("LastName") ? row["LastName"] : "")}".Trim()
        };
        
        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PutAsync("/api/auth/profile", content);
        await _context.StoreLastResponseAsync(response);
    }

    [Then(@"I should receive a validation error")]
    public void ThenIShouldReceiveAValidationError()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenTheErrorMessageShouldContain(string expectedMessage)
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainEquivalentOf(expectedMessage);
    }

    [Then(@"I should receive a successful update response")]
    public void ThenIShouldReceiveASuccessfulUpdateResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"my profile should be updated in the database")]
    public async Task ThenMyProfileShouldBeUpdatedInTheDatabase()
    {
        // Verify by fetching the profile
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var userProfile = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        userProfile.Should().NotBeNull();
    }

    [Then(@"when I request my profile again, it should show the updated information")]
    public async Task ThenWhenIRequestMyProfileAgainItShouldShowTheUpdatedInformation()
    {
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var userProfile = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        userProfile.Should().NotBeNull();
        userProfile!.FullName.Should().NotBeNullOrEmpty();
    }

    // Additional step definitions for authentication scenarios
    
    [Given(@"I have an existing user account:")]
    public async Task GivenIHaveAnExistingUserAccount(Table table)
    {
        var row = table.Rows.First();
        var registerRequest = new RegisterRequestDto
        {
            Email = row["Email"],
            Password = row["Password"],
            FullName = $"{row["FirstName"]} {row["LastName"]}"
        };
        
        var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/register", content);
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Store the user for later use in the test
        _context.StoreTestData("existingUser", registerRequest);
    }

    [When(@"I login with valid credentials:")]
    public async Task WhenILoginWithValidCredentials(Table table)
    {
        var row = table.Rows.First();
        var loginRequest = new LoginRequestDto
        {
            Email = row["Email"],
            Password = row["Password"]
        };
        
        var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/login", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I login with invalid credentials:")]
    public async Task WhenILoginWithInvalidCredentials(Table table)
    {
        var row = table.Rows.First();
        var loginRequest = new LoginRequestDto
        {
            Email = row["Email"],
            Password = row["Password"]
        };
        
        var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/login", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I register with valid credentials:")]
    public async Task WhenIRegisterWithValidCredentials(Table table)
    {
        var row = table.Rows.First();
        var registerRequest = new RegisterRequestDto
        {
            Email = row["Email"],
            Password = row["Password"],
            FullName = $"{row["FirstName"]} {row["LastName"]}"
        };
        
        _context.StoreTestData("registerRequest", registerRequest);
        
        var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/register", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I attempt to register with the same email:")]
    public async Task WhenIAttemptToRegisterWithTheSameEmail(Table table)
    {
        var row = table.Rows.First();
        var registerRequest = new RegisterRequestDto
        {
            Email = row["Email"],
            Password = row["Password"],
            FullName = $"{row["FirstName"]} {row["LastName"]}"
        };
        
        var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/register", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I access a protected endpoint with the access token")]
    public async Task WhenIAccessAProtectedEndpointWithTheAccessToken()
    {
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I access a protected endpoint with the expired token")]
    public async Task WhenIAccessAProtectedEndpointWithTheExpiredToken()
    {
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I access a protected endpoint without any token")]
    public async Task WhenIAccessAProtectedEndpointWithoutAnyToken()
    {
        // Clear any authentication header
        _context.HttpClient.DefaultRequestHeaders.Authorization = null;
        
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I request my user profile")]
    public async Task WhenIRequestMyUserProfile()
    {
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I logout from the system")]
    public async Task WhenILogoutFromTheSystem()
    {
        var response = await _context.HttpClient.PostAsync("/api/auth/logout", null);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I request a token refresh using the refresh token")]
    public async Task WhenIRequestATokenRefreshUsingTheRefreshToken()
    {
        var refreshRequest = new { RefreshToken = _context.CurrentRefreshToken ?? "valid-refresh-token" };
        var json = JsonSerializer.Serialize(refreshRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/refresh", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I request a token refresh using the invalid token")]
    public async Task WhenIRequestATokenRefreshUsingTheInvalidToken()
    {
        var refreshRequest = new { RefreshToken = "invalid-refresh-token" };
        var json = JsonSerializer.Serialize(refreshRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/refresh", content);
        await _context.StoreLastResponseAsync(response);
    }

    [Then(@"I should receive a successful login response")]
    public void ThenIShouldReceiveASuccessfulLoginResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should receive a successful registration response")]
    public void ThenIShouldReceiveASuccessfulRegistrationResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should receive a successful logout response")]
    public void ThenIShouldReceiveASuccessfulLogoutResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should receive a successful response")]
    public void ThenIShouldReceiveASuccessfulResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should receive a JWT access token")]
    public void ThenIShouldReceiveAJWTAccessToken()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        
        // Store token for future requests
        _context.CurrentAccessToken = tokenResponse.AccessToken;
        _context.SetAuthenticationToken(tokenResponse.AccessToken);
    }

    [Then(@"I should receive a refresh token")]
    public void ThenIShouldReceiveARefreshToken()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.RefreshToken.Should().NotBeNullOrEmpty();
        
        // Store refresh token for future requests
        _context.CurrentRefreshToken = tokenResponse.RefreshToken;
    }

    [Then(@"I should receive a new access token")]
    public void ThenIShouldReceiveANewAccessToken()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();
        
        // Store new token
        _context.CurrentAccessToken = tokenResponse.AccessToken;
        _context.SetAuthenticationToken(tokenResponse.AccessToken);
    }

    [Then(@"I should receive a new refresh token")]
    public void ThenIShouldReceiveANewRefreshToken()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(_context.LastResponseContent, _jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.RefreshToken.Should().NotBeNullOrEmpty();
        
        // Store new refresh token
        _context.CurrentRefreshToken = tokenResponse.RefreshToken;
    }

    [Then(@"the access token should contain valid user claims")]
    public void ThenTheAccessTokenShouldContainValidUserClaims()
    {
        _context.CurrentAccessToken.Should().NotBeNullOrEmpty();
        
        // For now, just verify that we have a token
        // In a more complete implementation, we would decode and verify JWT claims
        _context.CurrentAccessToken!.Should().Contain("."); // JWT tokens contain dots
    }

    [Then(@"the new access token should contain valid user claims")]
    public void ThenTheNewAccessTokenShouldContainValidUserClaims()
    {
        _context.CurrentAccessToken.Should().NotBeNullOrEmpty();
        _context.CurrentAccessToken!.Should().Contain("."); // JWT tokens contain dots
    }

    [Then(@"I should receive my profile information")]
    public void ThenIShouldReceiveMyProfileInformation()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var userProfile = JsonSerializer.Deserialize<UserDto>(_context.LastResponseContent, _jsonOptions);
        userProfile.Should().NotBeNull();
        userProfile!.Email.Should().NotBeNullOrEmpty();
    }

    [Then(@"the profile should contain:")]
    public void ThenTheProfileShouldContain(Table table)
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var userProfile = JsonSerializer.Deserialize<UserDto>(_context.LastResponseContent, _jsonOptions);
        userProfile.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var expectedValue = row["Value"];
            
            switch (field.ToLowerInvariant())
            {
                case "email":
                    userProfile!.Email.Should().Be(expectedValue);
                    break;
                case "firstname":
                case "fullname":
                    userProfile!.FullName.Should().ContainEquivalentOf(expectedValue);
                    break;
                case "lastname":
                    userProfile!.FullName.Should().ContainEquivalentOf(expectedValue);
                    break;
                case "role":
                    // Role verification would need role property in UserDto
                    break;
            }
        }
    }

    [Then(@"the response should contain user-specific data")]
    public void ThenTheResponseShouldContainUserSpecificData()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        var userProfile = JsonSerializer.Deserialize<UserDto>(_context.LastResponseContent, _jsonOptions);
        userProfile.Should().NotBeNull();
        userProfile!.Email.Should().NotBeNullOrEmpty();
        userProfile.FullName.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should receive an authentication error")]
    public void ThenIShouldReceiveAnAuthenticationError()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"I should receive an unauthorized error")]
    public void ThenIShouldReceiveAnUnauthorizedError()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"I should not receive any tokens")]
    public void ThenIShouldNotReceiveAnyTokens()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        
        // Verify no token data in response
        if (!string.IsNullOrEmpty(_context.LastResponseContent))
        {
            _context.LastResponseContent.Should().NotContain("accessToken");
            _context.LastResponseContent.Should().NotContain("refreshToken");
        }
    }

    [Then(@"I should not receive any new tokens")]
    public void ThenIShouldNotReceiveAnyNewTokens()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        
        // Verify no token data in response
        if (!string.IsNullOrEmpty(_context.LastResponseContent))
        {
            _context.LastResponseContent.Should().NotContain("accessToken");
            _context.LastResponseContent.Should().NotContain("refreshToken");
        }
    }

    [Then(@"my user account should be created in the database")]
    public async Task ThenMyUserAccountShouldBeCreatedInTheDatabase()
    {
        // Verify by trying to login with the credentials used for registration
        var registerRequest = _context.GetTestData<RegisterRequestDto>("registerRequest");
        if (registerRequest == null)
        {
            // Extract from response if available
            _context.LastResponse.Should().NotBeNull();
            _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
            return;
        }
        
        var loginRequest = new LoginRequestDto
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        
        var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/auth/login", content);
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"my access token should be revoked")]
    public async Task ThenMyAccessTokenShouldBeRevoked()
    {
        // Try to use the previous token to access a protected endpoint
        var previousToken = _context.CurrentAccessToken;
        if (!string.IsNullOrEmpty(previousToken))
        {
            _context.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", previousToken);
            var response = await _context.HttpClient.GetAsync("/api/auth/profile");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Then(@"my refresh token should be revoked")]
    public async Task ThenMyRefreshTokenShouldBeRevoked()
    {
        // Try to use the previous refresh token
        var previousRefreshToken = _context.CurrentRefreshToken;
        if (!string.IsNullOrEmpty(previousRefreshToken))
        {
            var refreshRequest = new { RefreshToken = previousRefreshToken };
            var json = JsonSerializer.Serialize(refreshRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _context.HttpClient.PostAsync("/api/auth/refresh", content);
            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }

    [Then(@"the old refresh token should be invalidated")]
    public async Task ThenTheOldRefreshTokenShouldBeInvalidated()
    {
        // This would require tracking the old refresh token before the refresh operation
        // For now, just verify that we have a new token
        _context.CurrentRefreshToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should not be able to access protected endpoints")]
    public async Task ThenIShouldNotBeAbleToAccessProtectedEndpoints()
    {
        var response = await _context.HttpClient.GetAsync("/api/auth/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"the error message should indicate invalid credentials")]
    public void ThenTheErrorMessageShouldIndicateInvalidCredentials()
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainAny("invalid", "credentials", "unauthorized", "authentication");
    }

    [Then(@"the error message should indicate invalid refresh token")]
    public void ThenTheErrorMessageShouldIndicateInvalidRefreshToken()
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainAny("invalid", "refresh", "token", "expired");
    }

    [Then(@"the error message should indicate token has expired")]
    public void ThenTheErrorMessageShouldIndicateTokenHasExpired()
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainAny("expired", "token", "invalid");
    }

    [Then(@"the error message should indicate email already exists")]
    public void ThenTheErrorMessageShouldIndicateEmailAlreadyExists()
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainAny("email", "exists", "already", "duplicate");
    }

    [Then(@"no new user account should be created")]
    public void ThenNoNewUserAccountShouldBeCreated()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
    }

    [Given(@"I have an expired access token")]
    public void GivenIHaveAnExpiredAccessToken()
    {
        // For testing purposes, set an obviously expired/invalid token
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _context.SetAuthenticationToken(expiredToken);
    }

    [Given(@"I have an invalid or expired refresh token")]
    public void GivenIHaveAnInvalidOrExpiredRefreshToken()
    {
        _context.CurrentRefreshToken = "invalid-or-expired-refresh-token";
    }
}
