using CadenceHub.Models;
using CadenceHub.Security;
using Microsoft.EntityFrameworkCore;

namespace CadenceHub.Services;

public sealed class AuthService
{
    private const string InitialAdminPassword = "Admin@123";

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Fail("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
        }

        await using var db = new CadenceHubContext();

        var account = await db.UserAccounts
            .Include(user => user.Staff)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Username == username);

        if (account is null)
        {
            return AuthenticationResult.Fail("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (!account.IsActive)
        {
            return AuthenticationResult.Fail("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị hệ thống.");
        }

        var verified = PasswordHasher.Verify(password, account.PasswordHash);
        var mustReplaceSetupPassword = false;

        if (!verified && CanUseInitialAdminPassword(account, password))
        {
            verified = true;
            mustReplaceSetupPassword = true;
        }

        if (!verified)
        {
            return AuthenticationResult.Fail("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        var roleCodes = account.UserRoles
            .Select(userRole => userRole.Role.Code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code)
            .ToArray();

        var roleNames = account.UserRoles
            .Select(userRole => userRole.Role.Name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToArray();

        if (roleCodes.Length == 0)
        {
            return AuthenticationResult.Fail("Tài khoản chưa được phân quyền.");
        }

        if (mustReplaceSetupPassword)
        {
            account.PasswordHash = PasswordHasher.Hash(password);
            account.UpdatedAt = DateTime.Now;
        }

        account.LastLoginAt = DateTime.Now;

        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = account.Id,
            ActionCode = "LOGIN_SUCCESS",
            EntityName = "user_accounts",
            EntityId = account.Id,
            NewValue = $"username={account.Username};roles={string.Join(",", roleCodes)}",
            Note = "Đăng nhập thành công."
        });

        await db.SaveChangesAsync();

        var user = new AuthenticatedUser(
            account.Id,
            account.Username,
            account.DisplayName,
            account.StaffId,
            account.Staff?.StaffCode,
            account.Staff?.FullName,
            roleCodes,
            roleNames);

        return AuthenticationResult.Success(user);
    }

    private static bool CanUseInitialAdminPassword(UserAccount account, string password)
    {
        return PasswordHasher.IsSetupPlaceholder(account.PasswordHash)
            && string.Equals(account.Username, "admin", StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, InitialAdminPassword, StringComparison.Ordinal);
    }
}
