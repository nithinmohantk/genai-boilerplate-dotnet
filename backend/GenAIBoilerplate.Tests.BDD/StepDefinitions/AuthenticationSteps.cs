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
}
