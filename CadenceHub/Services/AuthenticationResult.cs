using CadenceHub.Security;

namespace CadenceHub.Services;

public sealed class AuthenticationResult
{
    private AuthenticationResult(bool succeeded, string message, AuthenticatedUser? user)
    {
        Succeeded = succeeded;
        Message = message;
        User = user;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public AuthenticatedUser? User { get; }

    public static AuthenticationResult Success(AuthenticatedUser user)
    {
        return new AuthenticationResult(true, string.Empty, user);
    }

    public static AuthenticationResult Fail(string message)
    {
        return new AuthenticationResult(false, message, null);
    }
}
