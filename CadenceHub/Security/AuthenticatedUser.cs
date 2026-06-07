namespace CadenceHub.Security;

public sealed class AuthenticatedUser
{
    public AuthenticatedUser(
        int id,
        string username,
        string displayName,
        int? staffId,
        string? staffCode,
        string? staffFullName,
        IReadOnlyCollection<string> roleCodes,
        IReadOnlyCollection<string> roleNames)
    {
        Id = id;
        Username = username;
        DisplayName = displayName;
        StaffId = staffId;
        StaffCode = staffCode;
        StaffFullName = staffFullName;
        RoleCodes = roleCodes;
        RoleNames = roleNames;
    }

    public int Id { get; }

    public string Username { get; }

    public string DisplayName { get; }

    public int? StaffId { get; }

    public string? StaffCode { get; }

    public string? StaffFullName { get; }

    public IReadOnlyCollection<string> RoleCodes { get; }

    public IReadOnlyCollection<string> RoleNames { get; }

    public bool HasRole(string roleCode)
    {
        return RoleCodes.Any(code => string.Equals(code, roleCode, StringComparison.OrdinalIgnoreCase));
    }
}
