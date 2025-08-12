using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.API.Hubs;
using System.Text;

namespace GenAIBoilerplate.API.Controllers;

/// <summary>
/// Chat controller for AI interactions
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAIService _aiService;
    private readonly IJwtService _jwtService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IAIService aiService,
        IJwtService jwtService,
        IHubContext<ChatHub> hubContext,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _aiService = aiService;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all chat sessions for the current user
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<ChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetSessions(CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var sessions = await _chatService.GetUserSessionsAsync(userId.Value, cancellationToken);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions");
            return StatusCode(500, new { message = "An error occurred getting sessions" });
        }
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _chatService.CreateSessionAsync(userId.Value, request, cancellationToken);
            return Created($"sessions/{session.Id}", session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, new { message = "An error occurred creating the session" });
        }
    }

    /// <summary>
    /// Get a specific chat session
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatSessionDto>> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _chatService.GetSessionAsync(sessionId, userId.Value, cancellationToken);
            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred getting the session" });
        }
    }

    /// <summary>
    /// Update a chat session
    /// </summary>
    [HttpPut("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatSessionDto>> UpdateSession(Guid sessionId, [FromBody] UpdateSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _chatService.UpdateSessionAsync(sessionId, userId.Value, request, cancellationToken);
            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred updating the session" });
        }
    }

    /// <summary>
    /// Delete a chat session
    /// </summary>
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSession(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _chatService.DeleteSessionAsync(sessionId, userId.Value, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred deleting the session" });
        }
    }

    /// <summary>
    /// Get messages for a chat session
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(IEnumerable<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages(
        Guid sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var messages = await _chatService.GetMessagesAsync(sessionId, userId.Value, page, pageSize, cancellationToken);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred getting messages" });
        }
    }

    /// <summary>
    /// Send a message to the AI (non-streaming)
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatResponseDto>> SendMessage(
        Guid sessionId,
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _chatService.SendMessageAsync(sessionId, userId.Value, request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred sending the message" });
        }
    }

    /// <summary>
    /// Send a message to the AI with streaming response
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/messages/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessageStream(
        Guid sessionId,
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            Response.Headers.Add("Content-Type", "text/plain");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            await _chatService.SendMessageStreamAsync(sessionId, userId.Value, request, async (chunk) =>
            {
                await Response.WriteAsync(chunk, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }, cancellationToken);

            return new EmptyResult();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming message to session {SessionId}", sessionId);
            return StatusCode(500, new { message = "An error occurred streaming the message" });
        }
    }

    /// <summary>
    /// Get available AI models
    /// </summary>
    [HttpGet("models")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableModels(CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Use a dummy tenant ID for now - in production, get from user context
            var models = await _aiService.GetAvailableModelsAsync(Guid.Empty, cancellationToken);
            return Ok(models.Select(m => m.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return StatusCode(500, new { message = "An error occurred getting available models" });
        }
    }

    /// <summary>
    /// Get token count for a message
    /// </summary>
    [HttpPost("count-tokens")]
    [ProducesResponseType(typeof(TokenCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenCountDto>> CountTokens([FromBody] TokenCountRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenCount = await _aiService.CountTokensAsync(request.Text, request.Model, cancellationToken);
            return Ok(new TokenCountDto { Count = tokenCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting tokens");
            return StatusCode(500, new { message = "An error occurred counting tokens" });
        }
    }
}

/// <summary>
/// Token count request DTO
/// </summary>
public class TokenCountRequestDto
{
    public string Text { get; set; } = string.Empty;
    public string? Model { get; set; }
}

/// <summary>
/// Token count response DTO
/// </summary>
public class TokenCountDto
{
    public int Count { get; set; }
}
