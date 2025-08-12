using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Interfaces;
using GenAIBoilerplate.Tests.BDD.Support;
using TechTalk.SpecFlow;

namespace GenAIBoilerplate.Tests.BDD.StepDefinitions;

[Binding]
public class ChatSteps
{
    private readonly TestContext _context;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ChatSteps(TestContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Given(@"I have an active chat session with messages")]
    public async Task GivenIHaveAnActiveChatSessionWithMessages()
    {
        // Create a chat session
        var sessionRequest = new CreateSessionRequestDto
        {
            Title = "Test Chat Session",
            Model = "gpt-4"
        };
        
        var json = JsonSerializer.Serialize(sessionRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/chat/sessions", content);
        await _context.StoreLastResponseAsync(response);
        
        if (response.IsSuccessStatusCode)
        {
            var sessionResponse = JsonSerializer.Deserialize<ChatSessionDto>(_context.LastResponseContent, _jsonOptions);
            _context.StoreTestData("currentSession", sessionResponse);
            
            // Add some messages
            var messageRequest = new SendMessageRequestDto
            {
                Content = "Hello, this is a test message"
            };
            
            var messageJson = JsonSerializer.Serialize(messageRequest, _jsonOptions);
            var messageContent = new StringContent(messageJson, Encoding.UTF8, "application/json");
            
            await _context.HttpClient.PostAsync($"/api/chat/sessions/{sessionResponse!.Id}/messages", messageContent);
        }
    }

    [Given(@"I have a new chat session")]
    public async Task GivenIHaveANewChatSession()
    {
        var sessionRequest = new CreateSessionRequestDto
        {
            Title = "New Test Session",
            Model = "gpt-4"
        };
        
        var json = JsonSerializer.Serialize(sessionRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/chat/sessions", content);
        await _context.StoreLastResponseAsync(response);
        
        if (response.IsSuccessStatusCode)
        {
            var sessionResponse = JsonSerializer.Deserialize<ChatSessionDto>(_context.LastResponseContent, _jsonOptions);
            _context.StoreTestData("currentSession", sessionResponse);
        }
    }

    [Given(@"another user has a chat session")]
    public async Task GivenAnotherUserHasAChatSession()
    {
        // For testing purposes, we'll create a session with a different user context
        // In a real implementation, this would involve creating another user and their session
        var sessionId = Guid.NewGuid();
        _context.StoreTestData("otherUserSession", sessionId);
    }

    [When(@"I create a new chat session with title ""(.*)""")]
    public async Task WhenICreateANewChatSessionWithTitle(string title)
    {
        var sessionRequest = new CreateSessionRequestDto
        {
            Title = title,
            Model = "gpt-4"
        };
        
        var json = JsonSerializer.Serialize(sessionRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync("/api/chat/sessions", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I send a message ""(.*)"" to the session")]
    public async Task WhenISendAMessageToTheSession(string message)
    {
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        currentSession.Should().NotBeNull();
        
        var messageRequest = new SendMessageRequestDto
        {
            Content = message
        };
        
        var json = JsonSerializer.Serialize(messageRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync($"/api/chat/sessions/{currentSession!.Id}/messages", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I attempt to send a message to a non-existent session:")]
    public async Task WhenIAttemptToSendAMessageToANonExistentSession(Table table)
    {
        var row = table.Rows.First();
        var sessionId = Guid.Parse(row["SessionId"]);
        var content = row["Content"];
        
        var messageRequest = new SendMessageRequestDto
        {
            Content = content
        };
        
        var json = JsonSerializer.Serialize(messageRequest, _jsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync($"/api/chat/sessions/{sessionId}/messages", httpContent);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I archive the chat session")]
    public async Task WhenIArchiveTheChatSession()
    {
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        currentSession.Should().NotBeNull();
        
        var updateRequest = new UpdateSessionRequestDto
        {
            Title = currentSession!.Title,
            // Assuming IsActive = false means archived
        };
        
        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PutAsync($"/api/chat/sessions/{currentSession.Id}", content);
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I attempt to access their chat session")]
    public async Task WhenIAttemptToAccessTheirChatSession()
    {
        var otherUserSessionId = _context.GetTestDataValue<Guid>("otherUserSession");
        
        var response = await _context.HttpClient.GetAsync($"/api/chat/sessions/{otherUserSessionId}");
        await _context.StoreLastResponseAsync(response);
    }

    [When(@"I request my chat sessions")]
    public async Task WhenIRequestMyChatSessions()
    {
        var response = await _context.HttpClient.GetAsync("/api/chat/sessions");
        await _context.StoreLastResponseAsync(response);
    }

    [Then(@"a new chat session should be created")]
    public void ThenANewChatSessionShouldBeCreated()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        var sessionResponse = JsonSerializer.Deserialize<ChatSessionDto>(_context.LastResponseContent, _jsonOptions);
        sessionResponse.Should().NotBeNull();
        sessionResponse!.Id.Should().NotBeEmpty();
        sessionResponse.Title.Should().NotBeEmpty();
    }

    [Then(@"the session should contain my message")]
    public void ThenTheSessionShouldContainMyMessage()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        // The response should contain the sent message
        _context.LastResponseContent.Should().NotBeNullOrEmpty();
    }

    [Then(@"I should receive an AI response")]
    public void ThenIShouldReceiveAnAIResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(_context.LastResponseContent, _jsonOptions);
        chatResponse.Should().NotBeNull();
        chatResponse!.AssistantMessage.Should().NotBeNull();
        chatResponse.AssistantMessage.Content.Should().NotBeEmpty();
    }

    [Then(@"I should see a list of my sessions")]
    public void ThenIShouldSeeAListOfMySessions()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        var sessions = JsonSerializer.Deserialize<List<ChatSessionDto>>(_context.LastResponseContent, _jsonOptions);
        sessions.Should().NotBeNull();
        sessions!.Should().NotBeEmpty();
    }

    [Then(@"the sessions should be ordered by most recent")]
    public void ThenTheSessionsShouldBeOrderedByMostRecent()
    {
        var sessions = JsonSerializer.Deserialize<List<ChatSessionDto>>(_context.LastResponseContent, _jsonOptions);
        sessions.Should().NotBeNull();
        
        // Check if sessions are ordered by UpdatedAt or LastActivityAt in descending order
        for (int i = 0; i < sessions!.Count - 1; i++)
        {
            sessions[i].UpdatedAt.Should().BeAfter(sessions[i + 1].UpdatedAt);
        }
    }

    [Then(@"I should receive a not found error")]
    public void ThenIShouldReceiveANotFoundError()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Then(@"the error message should indicate session not found")]
    public void ThenTheErrorMessageShouldIndicateSessionNotFound()
    {
        _context.LastErrorMessage.Should().NotBeNullOrEmpty();
        _context.LastErrorMessage!.Should().ContainAny("not found", "session", "exist");
    }

    [Then(@"no message should be saved")]
    public async Task ThenNoMessageShouldBeSaved()
    {
        // Verify by checking that the session (if it existed) has no new messages
        // This is implicitly tested by the not found error, but we can add additional verification
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"the session should be marked as inactive")]
    public async Task ThenTheSessionShouldBeMarkedAsInactive()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        // Verify by getting the session and checking its status
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        var response = await _context.HttpClient.GetAsync($"/api/chat/sessions/{currentSession!.Id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<ChatSessionDto>(content, _jsonOptions);
            session!.IsActive.Should().BeFalse();
        }
    }

    [Then(@"the session should still be accessible for reading")]
    public async Task ThenTheSessionShouldStillBeAccessibleForReading()
    {
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        var response = await _context.HttpClient.GetAsync($"/api/chat/sessions/{currentSession!.Id}");
        
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should not be able to send new messages to the archived session")]
    public async Task ThenIShouldNotBeAbleToSendNewMessagesToTheArchivedSession()
    {
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        
        var messageRequest = new SendMessageRequestDto
        {
            Content = "This should fail"
        };
        
        var json = JsonSerializer.Serialize(messageRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync($"/api/chat/sessions/{currentSession!.Id}/messages", content);
        
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Then(@"the conversation history should remain intact")]
    public async Task ThenTheConversationHistoryShouldRemainIntact()
    {
        var currentSession = _context.GetTestData<ChatSessionDto>("currentSession");
        var response = await _context.HttpClient.GetAsync($"/api/chat/sessions/{currentSession!.Id}/messages");
        
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        var messages = JsonSerializer.Deserialize<List<ChatMessageDto>>(content, _jsonOptions);
        
        messages.Should().NotBeNull();
        messages!.Should().NotBeEmpty();
    }

    [Then(@"I should receive a forbidden error")]
    public void ThenIShouldReceiveAForbiddenError()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Then(@"I should not be able to view their session details")]
    public void ThenIShouldNotBeAbleToViewTheirSessionDetails()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeFalse();
        _context.LastResponse!.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Then(@"I should not be able to send messages to their session")]
    public async Task ThenIShouldNotBeAbleToSendMessagesToTheirSession()
    {
        var otherUserSessionId = _context.GetTestDataValue<Guid>("otherUserSession");
        
        var messageRequest = new SendMessageRequestDto
        {
            Content = "Unauthorized message"
        };
        
        var json = JsonSerializer.Serialize(messageRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _context.HttpClient.PostAsync($"/api/chat/sessions/{otherUserSessionId}/messages", content);
        
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}
