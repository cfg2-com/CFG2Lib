using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CFG2.Utils.SecLib;

/// <summary>
/// Provides functionality for encrypting and decrypting data using a password-based key derivation function (PBKDF2).
/// </summary>
/// <remarks>This class implements encryption and decryption using AES in CBC mode with PKCS7 padding.  The
/// cryptographic key is derived from the provided password using PBKDF2 with a randomly generated salt. The salt and
/// initialization vector (IV) are stored alongside the encrypted data to allow decryption.</remarks>
internal class PasswordAlgKDF : AbstractPasswordAlg
{
    private const int PBKDF2_ITERATIONS = 100000;
    private const int KEY_SIZE_BYTES = 32;
    private const int SALT_SIZE_BYTES = 16;

    public PasswordAlgKDF(string password) : base(password)
    {
    }

    public override string Decrypt(string encryptedValue)
    {
        byte[] cipherBlob = Convert.FromBase64String(encryptedValue);
        if (cipherBlob == null || cipherBlob.Length == 0) throw new ArgumentNullException(nameof(cipherBlob));

        using (MemoryStream msInput = new MemoryStream(cipherBlob))
        {
            // 1. Read salt, IV, and encrypted data from the combined blob
            byte[] salt = ReadByteArrayWithLength(msInput);
            byte[] iv = ReadByteArrayWithLength(msInput);
            byte[] encryptedData = new byte[msInput.Length - msInput.Position];
            msInput.Read(encryptedData, 0, encryptedData.Length);

            // 2. Derive the key using the password and the *retrieved* salt
            byte[] key = DeriveKeyFromPassword(GetPassword(), salt, PBKDF2_ITERATIONS, KEY_SIZE_BYTES);

            // 3. Decrypt the data with AES
            string plainText = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = KEY_SIZE_BYTES * 8;
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new MemoryStream(encryptedData);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                plainText = srDecrypt.ReadToEnd();
            }
            return plainText;
        }
    }

    public override string Encrypt(string decryptedValue)
    {
        if (string.IsNullOrEmpty(decryptedValue)) throw new ArgumentNullException(nameof(decryptedValue));

        // 1. Generate a random salt for this encryption operation
        byte[] salt = GenerateRandomBytes(SALT_SIZE_BYTES);

        // 2. Derive the key from the password and salt using PBKDF2
        byte[] key = DeriveKeyFromPassword(GetPassword(), salt, PBKDF2_ITERATIONS, KEY_SIZE_BYTES);

        // 3. Generate a random IV for AES encryption
        byte[] iv = GenerateRandomBytes(16); // AES block size is 16 bytes (128 bits)

        // 4. Encrypt the plaintext with AES
        byte[] encryptedData;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.KeySize = KEY_SIZE_BYTES * 8; // Convert bytes to bits
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC; // CBC is a common and secure mode
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(decryptedValue);
            }
            encryptedData = msEncrypt.ToArray();
        }

        // 5. Combine salt, IV, and encrypted data for storage/transmission
        // The salt and IV are not secret and must be stored with the ciphertext.
        using (MemoryStream msOutput = new MemoryStream())
        {
            WriteByteArrayWithLength(msOutput, salt);
            WriteByteArrayWithLength(msOutput, iv);
            msOutput.Write(encryptedData, 0, encryptedData.Length);
            return Convert.ToBase64String(msOutput.ToArray());
        }
    }

    /// <summary>
    /// Generates cryptographically strong random bytes.
    /// </summary>
    private static byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    /// <summary>
    /// Derives a cryptographic key from a password and salt using PBKDF2.
    /// </summary>
    private static byte[] DeriveKeyFromPassword(string password, byte[] salt, int iterations, int keyLength)
    {
        using (Rfc2898DeriveBytes pbkdf2 = new(password, salt, iterations, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(keyLength);
        }
    }

    // Helper methods to write/read byte arrays with their length prefixes
    private static void WriteByteArrayWithLength(MemoryStream ms, byte[] array)
    {
        ms.Write(BitConverter.GetBytes(array.Length), 0, 4); // Write length as int
        ms.Write(array, 0, array.Length);
    }

    private static byte[] ReadByteArrayWithLength(MemoryStream ms)
    {
        byte[] lengthBytes = new byte[4];
        int bytesRead = ms.Read(lengthBytes, 0, 4);
        if (bytesRead != 4)
        {
            throw new EndOfStreamException("Could not read length prefix.");
        }
        int length = BitConverter.ToInt32(lengthBytes, 0);
        if (length < 0 || ms.Length - ms.Position < length) // Basic check for invalid length or insufficient data
        {
            throw new InvalidDataException("Invalid array length or insufficient data in stream.");
        }
        byte[] array = new byte[length];
        bytesRead = ms.Read(array, 0, length);
        if (bytesRead != length)
        {
            throw new EndOfStreamException("Could not read full array.");
        }
        return array;
    }
}
