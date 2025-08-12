using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Entities;
using GenAIBoilerplate.Core.Enums;

namespace GenAIBoilerplate.API.Controllers;

/// <summary>
/// Tenant management controller
/// </summary>
[ApiController]
[Route("api/[controller]s")]
[Produces("application/json")]
[Authorize]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<TenantController> _logger;

    public TenantController(ITenantService tenantService, IJwtService jwtService, ILogger<TenantController> logger)
    {
        _tenantService = tenantService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's tenants
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetUserTenants(CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var tenants = await _tenantService.GetUserTenantsAsync(userId.Value, cancellationToken);
            return Ok(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user tenants");
            return StatusCode(500, new { message = "An error occurred getting tenants" });
        }
    }

    /// <summary>
    /// Get a specific tenant
    /// </summary>
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var tenant = await _tenantService.GetTenantAsync(tenantId, userId.Value, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(tenant);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred getting the tenant" });
        }
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantRequestDto request, CancellationToken cancellationToken)
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

            var tenant = await _tenantService.CreateTenantAsync(userId.Value, request, cancellationToken);
            return Created($"tenant/{tenant.Id}", tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return StatusCode(500, new { message = "An error occurred creating the tenant" });
        }
    }

    /// <summary>
    /// Update a tenant
    /// </summary>
    [HttpPut("{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid tenantId, [FromBody] UpdateTenantRequestDto request, CancellationToken cancellationToken)
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

            var tenant = await _tenantService.UpdateTenantAsync(tenantId, userId.Value, request, cancellationToken);
            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(tenant);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred updating the tenant" });
        }
    }

    /// <summary>
    /// Delete a tenant
    /// </summary>
    [HttpDelete("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _tenantService.DeleteTenantAsync(tenantId, userId.Value, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred deleting the tenant" });
        }
    }

    /// <summary>
    /// Get tenant API keys
    /// </summary>
    [HttpGet("{tenantId:guid}/api-keys")]
    [ProducesResponseType(typeof(IEnumerable<TenantApiKeyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TenantApiKeyDto>>> GetTenantApiKeys(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var apiKeys = await _tenantService.GetTenantApiKeysAsync(tenantId, userId.Value, cancellationToken);
            return Ok(apiKeys);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API keys for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred getting API keys" });
        }
    }

    /// <summary>
    /// Create a new tenant API key
    /// </summary>
    [HttpPost("{tenantId:guid}/api-keys")]
    [ProducesResponseType(typeof(TenantApiKeyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantApiKeyDto>> CreateApiKey(Guid tenantId, [FromBody] CreateApiKeyRequestDto request, CancellationToken cancellationToken)
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

            var apiKey = await _tenantService.CreateApiKeyAsync(tenantId, userId.Value, request, cancellationToken);
            return Created($"tenant/{tenantId}/api-keys/{apiKey.Id}", apiKey);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred creating the API key" });
        }
    }

    /// <summary>
    /// Revoke an API key
    /// </summary>
    [HttpDelete("{tenantId:guid}/api-keys/{apiKeyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeApiKey(Guid tenantId, Guid apiKeyId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _tenantService.RevokeApiKeyAsync(tenantId, apiKeyId, userId.Value, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking API key {ApiKeyId} for tenant {TenantId}", apiKeyId, tenantId);
            return StatusCode(500, new { message = "An error occurred revoking the API key" });
        }
    }

    /// <summary>
    /// Get tenant usage statistics
    /// </summary>
    [HttpGet("{tenantId:guid}/usage")]
    [ProducesResponseType(typeof(TenantUsageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantUsageDto>> GetTenantUsage(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var usage = await _tenantService.GetTenantUsageAsync(tenantId, userId.Value, startDate.Value, endDate.Value, cancellationToken);
            return Ok(usage);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred getting tenant usage" });
        }
    }

    /// <summary>
    /// Add user to tenant
    /// </summary>
    [HttpPost("{tenantId:guid}/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddUserToTenant(Guid tenantId, [FromBody] AddUserToTenantRequestDto request, CancellationToken cancellationToken)
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

            await _tenantService.AddUserToTenantAsync(tenantId, request.Email, request.Role, userId.Value, cancellationToken);
            return Ok(new { message = "User added to tenant successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred adding user to tenant" });
        }
    }

    /// <summary>
    /// Remove user from tenant
    /// </summary>
    [HttpDelete("{tenantId:guid}/users/{userIdToRemove:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserFromTenant(Guid tenantId, Guid userIdToRemove, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _tenantService.RemoveUserFromTenantAsync(tenantId, userIdToRemove, userId.Value, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserIdToRemove} from tenant {TenantId}", userIdToRemove, tenantId);
            return StatusCode(500, new { message = "An error occurred removing user from tenant" });
        }
    }
}

/// <summary>
/// Add user to tenant request DTO
/// </summary>
public class AddUserToTenantRequestDto
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}

