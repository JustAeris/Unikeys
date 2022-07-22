using System.Security.Cryptography;
using System.Text;

namespace Unikeys.Core.FileEncryption;

public static class PasswordGenerator
{
    private static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*".ToCharArray();
    private static readonly char[] Numbers = "1234567890".ToCharArray();
    private static readonly char[] SpecialChars = "!@#$%^&*".ToCharArray();

    /// <summary>
    /// Generates a cryptographically strong password.
    /// </summary>
    /// <param name="length">Length of the password to generate</param>
    /// <param name="minNumbers">Minimum number of numbers to include in the result</param>
    /// <param name="minSpecialChars">Minimum number of special</param>
    /// <returns>The generated password</returns>
    /// <exception cref="ArgumentException">Invalid amount of <i>minNumbers</i> or/and <i>minSpecialChars</i> OR the length is too short.</exception>
    public static string GetNewPassword(int length = 16, int minNumbers = 3, int minSpecialChars = 3)
    {
        if (length < 1)
            throw new ArgumentException("Password length must be at least 1 character long", nameof(length));

        if (minNumbers > length || minSpecialChars > length || minNumbers + minSpecialChars > length)
            throw new ArgumentException("Password length is too short for the given minimum number of numbers/special characters.",
                nameof(minNumbers) + " " + nameof(minSpecialChars));

        var result = new StringBuilder(length);
        while (!(result.ToString().Count(c => Numbers.Contains(c)) >= minNumbers && result.ToString().Count(c => SpecialChars.Contains(c)) >= minSpecialChars))
        {
            result.Clear();
            for (var i = 0; i < length; i++)
            {
                var index = RandomNumberGenerator.GetInt32(70);
                result.Append(Chars[index]);
            }
        }
        return result.ToString();
    }
}
