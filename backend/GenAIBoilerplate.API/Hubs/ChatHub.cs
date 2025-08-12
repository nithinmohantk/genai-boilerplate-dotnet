using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using GenAIBoilerplate.Application.Interfaces;
using System.Security.Claims;

namespace GenAIBoilerplate.API.Hubs;

/// <summary>
/// SignalR hub for real-time chat functionality
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IJwtService jwtService, ILogger<ChatHub> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Called when client connects to hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(Context.User);
            if (userId != null)
            {
                // Add user to their personal group for notifications
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("User connected without valid user ID in token");
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    /// <summary>
    /// Called when client disconnects from hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(Context.User);
            if (userId != null)
            {
                // Remove user from their personal group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "User disconnected with exception from connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Join a chat session group for real-time updates
    /// </summary>
    /// <param name="sessionId">The session ID to join</param>
    public async Task JoinSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                await Clients.Caller.SendAsync("Error", "Session ID is required");
                return;
            }

            var userId = _jwtService.GetUserIdFromClaims(Context.User);
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            // TODO: Verify user has access to this session
            // This would require injecting a chat service to check ownership

            var groupName = $"session_{sessionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("User {UserId} joined session {SessionId} with connection {ConnectionId}", 
                userId, sessionId, Context.ConnectionId);

            await Clients.Caller.SendAsync("JoinedSession", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId} for connection {ConnectionId}", sessionId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to join session");
        }
    }

    /// <summary>
    /// Leave a chat session group
    /// </summary>
    /// <param name="sessionId">The session ID to leave</param>
    public async Task LeaveSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                await Clients.Caller.SendAsync("Error", "Session ID is required");
                return;
            }

            var userId = _jwtService.GetUserIdFromClaims(Context.User);
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            var groupName = $"session_{sessionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("User {UserId} left session {SessionId} with connection {ConnectionId}", 
                userId, sessionId, Context.ConnectionId);

            await Clients.Caller.SendAsync("LeftSession", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId} for connection {ConnectionId}", sessionId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to leave session");
        }
    }

    /// <summary>
    /// Send typing indicator to other users in the session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <param name="isTyping">Whether the user is typing</param>
    public async Task SendTypingIndicator(string sessionId, bool isTyping)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            var userId = _jwtService.GetUserIdFromClaims(Context.User);
            if (userId == null)
            {
                return;
            }

            var groupName = $"session_{sessionId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId)
                .SendAsync("TypingIndicator", sessionId, userId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator for session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// Send a ping to test connection
    /// </summary>
    public async Task Ping()
    {
        try
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ping for connection {ConnectionId}", Context.ConnectionId);
        }
    }
}
