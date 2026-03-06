namespace Infrastructure.Authorization;

internal sealed class PermissionProvider
{
    /// <summary>
    /// Default permissions granted to all authenticated users until role/permission store is implemented.
    /// Replace with database or claim-based lookup when adding proper authorization.
    /// </summary>
    private static readonly HashSet<string> DefaultAuthenticatedPermissions = new(StringComparer.Ordinal)
    {
        "POS.Products:read",
        "POS.Products:write",
        "POS.Products:bulk",
        "POS.Orders:write",
        "POS.Reports:read",
        "users:access",
    };

    public Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        // TODO: Replace with real implementation (e.g. load from UserRoles -> RolePermissions in DB).
        // For now, grant default permissions to any authenticated user so the API is usable.
        HashSet<string> permissionsSet = new(DefaultAuthenticatedPermissions);
        return Task.FromResult(permissionsSet);
    }
}
