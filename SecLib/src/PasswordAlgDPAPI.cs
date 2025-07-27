using CFG2.Utils.LogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CFG2.Utils.SecLib;

/// <summary>
/// Implements password-based encryption and decryption using the Data Protection API (DPAPI).
/// </summary>
/// <remarks>This impelementation is ONLY suitable for use by a single user on single machine. 
/// If you need to share encrypted data between users or machines, consider using PasswordAlgKDF.</remarks>
internal class PasswordAlgDPAPI : AbstractPasswordAlg
{
    public PasswordAlgDPAPI(string password) : base(password)
    {
    }

    /// <summary>
    /// Decrypts a Base64-encoded, encrypted string using the current user's data protection scope.
    /// </summary>
    /// <remarks>This method uses the <see cref="System.Security.Cryptography.ProtectedData"/> class to
    /// decrypt the input string. The decryption is performed using the <see
    /// cref="System.Security.Cryptography.DataProtectionScope.CurrentUser"/> scope, meaning the data can only be
    /// decrypted by the same user on the same machine that encrypted it.</remarks>
    /// <param name="encryptedValue">The Base64-encoded string to decrypt. Must not be null or empty.</param>
    /// <returns>The decrypted string in UTF-8 encoding. Returns an empty string if <paramref name="encryptedValue"/> is null or
    /// empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the decryption process fails, such as when the data is corrupted or was encrypted by a different user
    /// or machine.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="encryptedValue"/> is not a valid Base64-encoded string.</exception>
    public override string Decrypt(string encryptedValue)
    {
        if (string.IsNullOrEmpty(encryptedValue))
        {
            return string.Empty;
        }

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, GetPasswordByteArray(), DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException ex)
        {
            // Log the exception
            Logger.Trace($"Decryption error: {ex.Message}");
            throw new InvalidOperationException("Failed to decrypt data. Data might be corrupted or encrypted by a different user/machine.", ex);
        }
        catch (FormatException ex)
        {
            // Log the exception if the input is not a valid Base64 string
            Logger.Trace($"Decryption error: Invalid Base64 string. {ex.Message}");
            throw new ArgumentException("Input string is not a valid Base64 format.", ex);
        }
    }

    /// <summary>
    /// Encrypts the specified plaintext value using the current user's data protection scope.
    /// </summary>
    /// <remarks>This method uses the <see cref="System.Security.Cryptography.ProtectedData"/> class to
    /// encrypt the input value. The encryption is tied to the current user and cannot be decrypted by other users or on
    /// other machines.</remarks>
    /// <param name="decryptedValue">The plaintext value to encrypt. Cannot be null or empty.</param>
    /// <returns>A Base64-encoded string representing the encrypted value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="decryptedValue"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the encryption process fails.</exception>
    public override string Encrypt(string decryptedValue)
    {
        if (string.IsNullOrEmpty(decryptedValue))
        {
            throw new ArgumentNullException(nameof(decryptedValue), "Password cannot be null or empty.");
        }

        try
        {
            byte[] decryptedBytes = Encoding.UTF8.GetBytes(decryptedValue);
            byte[] encryptedBytes = ProtectedData.Protect(decryptedBytes, GetPasswordByteArray(), DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            Logger.Trace($"Encryption error: {ex.Message}");
            throw new InvalidOperationException("Failed to encrypt data.", ex);
        }
    }
}
