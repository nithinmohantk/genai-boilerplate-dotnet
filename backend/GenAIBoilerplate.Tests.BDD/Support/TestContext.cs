using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using AutoFixture;
using GenAIBoilerplate.API;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Core.Entities;

namespace GenAIBoilerplate.Tests.BDD.Support;

/// <summary>
/// Shared test context for maintaining state across BDD test steps
/// </summary>
public class TestContext
{
    public TestWebApplicationFactory<Program> Factory { get; set; } = null!;
    public HttpClient HttpClient { get; set; } = null!;
    public IServiceScope ServiceScope { get; set; } = null!;
    public ILogger<TestContext> Logger { get; set; } = null!;
    public Fixture AutoFixture { get; set; } = new();
    
    // Authentication state
    public string? CurrentAccessToken { get; set; }
    public string? CurrentRefreshToken { get; set; }
    public UserDto? CurrentUser { get; set; }
    public Guid? CurrentUserId { get; set; }
    public Guid? CurrentTenantId { get; set; }
    
    // Test data storage
    public Dictionary<string, object> TestData { get; set; } = new();
    public List<User> CreatedUsers { get; set; } = new();
    public List<Tenant> CreatedTenants { get; set; } = new();
    public List<ChatSession> CreatedChatSessions { get; set; } = new();
    public List<ChatMessage> CreatedChatMessages { get; set; } = new();
    
    // HTTP response state
    public HttpResponseMessage? LastResponse { get; set; }
    public string? LastResponseContent { get; set; }
    public int? LastStatusCode => LastResponse?.StatusCode != null ? (int)LastResponse.StatusCode : null;
    
    // Error state
    public Exception? LastException { get; set; }
    public string? LastErrorMessage { get; set; }
    public Dictionary<string, string[]>? LastValidationErrors { get; set; }
    
    // Settings and configuration
    public Dictionary<string, string> TestSettings { get; set; } = new();
    
    public TestContext()
    {
        InitializeAutoFixture();
        InitializeTestSettings();
    }
    
    /// <summary>
    /// Initialize the test context with web application factory and HTTP client
    /// </summary>
    public void Initialize(TestWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        ServiceScope = factory.Services.CreateScope();
        Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<TestContext>>();
        
        // Configure HTTP client defaults
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    /// <summary>
    /// Set authentication token for subsequent requests
    /// </summary>
    public void SetAuthenticationToken(string accessToken)
    {
        CurrentAccessToken = accessToken;
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    /// <summary>
    /// Clear authentication token
    /// </summary>
    public void ClearAuthentication()
    {
        CurrentAccessToken = null;
        CurrentRefreshToken = null;
        CurrentUser = null;
        CurrentUserId = null;
        HttpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    /// <summary>
    /// Store test data with a key for later retrieval
    /// </summary>
    public void StoreTestData(string key, object value)
    {
        TestData[key] = value;
    }
    
    /// <summary>
    /// Retrieve stored test data
    /// </summary>
    public T? GetTestData<T>(string key) where T : class
    {
        return TestData.TryGetValue(key, out var value) ? value as T : null;
    }
    
    /// <summary>
    /// Retrieve stored test data for value types
    /// </summary>
    public T? GetTestDataValue<T>(string key) where T : struct
    {
        if (TestData.TryGetValue(key, out var value))
        {
            if (value is T directValue)
                return directValue;
            if (value != null && typeof(T).IsAssignableFrom(value.GetType()))
                return (T)value;
        }
        return null;
    }
    
    /// <summary>
    /// Store the last HTTP response and content
    /// </summary>
    public async Task StoreLastResponseAsync(HttpResponseMessage response)
    {
        LastResponse = response;
        LastResponseContent = await response.Content.ReadAsStringAsync();
        
        Logger?.LogDebug("HTTP Response - Status: {StatusCode}, Content: {Content}", 
            response.StatusCode, LastResponseContent);
            
        // Parse error responses
        if (!response.IsSuccessStatusCode && !string.IsNullOrEmpty(LastResponseContent))
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(LastResponseContent);
                if (errorResponse?.TryGetValue("message", out var message) == true)
                {
                    LastErrorMessage = message.ToString();
                }
                
                if (errorResponse?.TryGetValue("errors", out var errors) == true)
                {
                    var errorsJson = JsonSerializer.Serialize(errors);
                    LastValidationErrors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsJson);
                }
            }
            catch (JsonException)
            {
                LastErrorMessage = LastResponseContent;
            }
        }
    }
    
    /// <summary>
    /// Clear all stored state for the next test
    /// </summary>
    public void ClearState()
    {
        ClearAuthentication();
        TestData.Clear();
        CreatedUsers.Clear();
        CreatedTenants.Clear();
        CreatedChatSessions.Clear();
        CreatedChatMessages.Clear();
        LastResponse?.Dispose();
        LastResponse = null;
        LastResponseContent = null;
        LastException = null;
        LastErrorMessage = null;
        LastValidationErrors = null;
    }
    
    /// <summary>
    /// Create test user data using AutoFixture
    /// </summary>
    public RegisterRequestDto CreateTestUser(string? email = null, string? password = null)
    {
        var user = AutoFixture.Create<RegisterRequestDto>();
        if (!string.IsNullOrEmpty(email)) user.Email = email;
        if (!string.IsNullOrEmpty(password)) user.Password = password;
        return user;
    }
    
    /// <summary>
    /// Create test tenant data using AutoFixture
    /// </summary>
    public CreateTenantRequestDto CreateTestTenant(string? name = null)
    {
        var tenant = AutoFixture.Create<CreateTenantRequestDto>();
        if (!string.IsNullOrEmpty(name)) tenant.Name = name;
        return tenant;
    }
    
    /// <summary>
    /// Create test chat session data using AutoFixture
    /// </summary>
    public CreateSessionRequestDto CreateTestChatSession(string? title = null, string? model = null)
    {
        var session = AutoFixture.Create<CreateSessionRequestDto>();
        if (!string.IsNullOrEmpty(title)) session.Title = title;
        if (!string.IsNullOrEmpty(model)) session.Model = model;
        return session;
    }
    
    /// <summary>
    /// Get a service from the DI container
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return ServiceScope.ServiceProvider.GetRequiredService<T>();
    }
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        HttpClient?.Dispose();
        ServiceScope?.Dispose();
        LastResponse?.Dispose();
    }
    
    private void InitializeAutoFixture()
    {
        AutoFixture.Customize<RegisterRequestDto>(c => c
            .With(x => x.Email, () => AutoFixture.Create<string>() + "@example.com")
            .With(x => x.Password, "TestPass123!")
        );
        
        AutoFixture.Customize<CreateTenantRequestDto>(c => c
            .With(x => x.Name, () => "Test Tenant " + AutoFixture.Create<string>()[..8])
        );
        
        AutoFixture.Customize<CreateSessionRequestDto>(c => c
            .With(x => x.Title, () => "Test Session " + AutoFixture.Create<string>()[..8])
            .With(x => x.Model, "gpt-4")
        );
    }
    
    private void InitializeTestSettings()
    {
        TestSettings["BaseUrl"] = "https://localhost:7001";
        TestSettings["ApiTimeout"] = "30000";
        TestSettings["RetryCount"] = "3";
        TestSettings["RetryDelay"] = "1000";
        TestSettings["DefaultPassword"] = "TestPass123!";
    }
}
