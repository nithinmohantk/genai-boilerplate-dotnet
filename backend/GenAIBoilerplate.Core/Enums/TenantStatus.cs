namespace GenAIBoilerplate.Core.Enums;

/// <summary>
/// Tenant status
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is active and operational
    /// </summary>
    Active,
    
    /// <summary>
    /// Tenant is suspended
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Tenant is pending activation
    /// </summary>
    Pending,
    
    /// <summary>
    /// Tenant has been cancelled
    /// </summary>
    Cancelled
}
