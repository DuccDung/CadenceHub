using System.Security.Cryptography;
using System.Text;

namespace CadenceHub.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "PBKDF2";

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            "$",
            Prefix,
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (TryVerifyPbkdf2(password, storedHash))
        {
            return true;
        }

        if (TryVerifySha256Hex(password, storedHash))
        {
            return true;
        }

        return false;
    }

    public static bool IsSetupPlaceholder(string storedHash)
    {
        return string.Equals(storedHash, "CHANGE_ME_HASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryVerifyPbkdf2(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !string.Equals(parts[0], Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations < 10_000)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryVerifySha256Hex(string password, string storedHash)
    {
        if (storedHash.Length != 64 || storedHash.Any(c => !Uri.IsHexDigit(c)))
        {
            return false;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var actual = Convert.ToHexString(bytes);
        return string.Equals(actual, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
