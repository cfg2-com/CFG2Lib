using System.Text;
using System.Security.Cryptography;

namespace CFG2.Utils.SecLib;

public class SecLib
{
    private static readonly byte[] entropy = Encoding.UTF8.GetBytes("This!$someS3riouSenTropica+i0n");

    /// <summary>
    /// Stores a key-value pair in the specified file, encrypting the value before saving.
    /// </summary>
    /// <remarks>If the specified key already exists in the file, its associated value will be updated with
    /// the new encrypted value. If the key does not exist, a new key-value pair will be appended to the file. The
    /// method handles file I/O errors gracefully, returning <see langword="false"/> if an error occurs during reading
    /// or writing.</remarks>
    /// <param name="filePath">The path to the file where the key-value pair will be stored. The file must exist.</param>
    /// <param name="key">The key to associate with the value. Cannot be null or empty.</param>
    /// <param name="value">The value to store, which will be encrypted before saving. Cannot be null or empty.</param>
    /// <returns><see langword="true"/> if the key-value pair was successfully stored; otherwise, <see langword="false"/> if an
    /// error occurred during the operation.</returns>
    /// <exception cref="Exception">Thrown if <paramref name="key"/> or <paramref name="value"/> is null or empty, or if the file specified by
    /// <paramref name="filePath"/> does not exist.</exception>
    public static bool Store(string filePath, string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("Key cannot be null or empty.");
        }
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception("Value cannot be null or empty.");
        }
        if (!File.Exists(filePath))
        {
            throw new Exception($"File not found: {filePath}");
        }

        string encryptedValue = Encrypt(value);
        string lineToWrite = $"{key}={encryptedValue}";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            bool keyFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith($"{key}="))
                {
                    lines[i] = lineToWrite;
                    keyFound = true;
                    break;
                }
            }

            if (!keyFound)
            {
                File.AppendAllText(filePath, lineToWrite + Environment.NewLine);
            }
            else
            {
                File.WriteAllLines(filePath, lines);
            }
            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error storing data: {ex.Message}");
            return false;
        }
        catch (InvalidOperationException) // Propagated from Encrypt
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves the decrypted value associated with the specified key from the given file.
    /// </summary>
    /// <remarks>The method reads the file line by line, searching for a line that starts with the specified
    /// key followed by an equals sign ('='). If found, the value is decrypted and returned. If the key is not found, or
    /// if an error occurs during decryption, the method returns <see langword="null"/>.</remarks>
    /// <param name="filePath">The path to the file containing key-value pairs.</param>
    /// <param name="key">The key whose associated value is to be retrieved. Cannot be null or empty.</param>
    /// <returns>The decrypted value associated with the specified key, or <see langword="null"/> if the key is not found or an
    /// error occurs during retrieval or decryption.</returns>
    /// <exception cref="Exception"></exception>
    public static string Retrieve(string filePath, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("key cannot be null or empty");
        }
        if (!File.Exists(filePath))
        {
            throw new Exception($"File not found: {filePath}");
        }

        try
        {
            foreach (string line in File.ReadLines(filePath))
            {
                if (line.StartsWith($"{key}="))
                {
                    string encryptedValue = line.Substring(key.Length + 1);
                    return Decrypt(encryptedValue);
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error retrieving data: {ex.Message}");
        }
        catch (InvalidOperationException) // Propagated from Decrypt
        {
            // Decryption failed, likely due to corrupted data or wrong user/machine
        }
        catch (ArgumentException) // Propagated from Decrypt (invalid Base64)
        {
            // Input was not valid Base64
        }
        return null; // Key not found or an error occurred
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
    public static string Encrypt(string decryptedValue)
    {
        if (string.IsNullOrEmpty(decryptedValue))
        {
            throw new ArgumentNullException(nameof(decryptedValue), "Password cannot be null or empty.");
        }

        try
        {
            byte[] decryptedBytes = Encoding.UTF8.GetBytes(decryptedValue);
            byte[] encryptedBytes = ProtectedData.Protect(decryptedBytes, entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            // Log the exception (e.g., using a logging framework)
            Console.WriteLine($"Encryption error: {ex.Message}");
            throw new InvalidOperationException("Failed to encrypt data.", ex);
        }
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
    public static string Decrypt(string encryptedValue)
    {
        if (string.IsNullOrEmpty(encryptedValue))
        {
            return string.Empty;
        }

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException ex)
        {
            // Log the exception
            Console.WriteLine($"Decryption error: {ex.Message}");
            throw new InvalidOperationException("Failed to decrypt data. Data might be corrupted or encrypted by a different user/machine.", ex);
        }
        catch (FormatException ex)
        {
            // Log the exception if the input is not a valid Base64 string
            Console.WriteLine($"Decryption error: Invalid Base64 string. {ex.Message}");
            throw new ArgumentException("Input string is not a valid Base64 format.", ex);
        }
    }
}
