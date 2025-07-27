namespace CFG2.Utils.SecLib;

public class SecUtil
{
    public static string Encrypt(string encryptionPassword, string decryptedValue)
    {
        PasswordAlgKDF alg = new PasswordAlgKDF(encryptionPassword);
        return alg.Encrypt(decryptedValue);
    }

    public static string Decrypt(string encryptionPassword, string encryptedValue)
    {
        PasswordAlgKDF alg = new PasswordAlgKDF(encryptionPassword);
        return alg.Decrypt(encryptedValue);
    }
}
