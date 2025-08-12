namespace GenAIBoilerplate.Core.Enums;

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user
    /// </summary>
    User,
    
    /// <summary>
    /// Administrator
    /// </summary>
    Admin,
    
    /// <summary>
    /// System-wide administrator
    /// </summary>
    SuperAdmin,
    
    /// <summary>
    /// Tenant administrator
    /// </summary>
    TenantAdmin,
    
    /// <summary>
    /// Regular tenant user
    /// </summary>
    TenantUser,
    
    /// <summary>
    /// Read-only tenant user
    /// </summary>
    TenantViewer
}
