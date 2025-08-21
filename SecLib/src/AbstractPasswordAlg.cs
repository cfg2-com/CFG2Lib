using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFG2.Utils.SecLib;

internal abstract class AbstractPasswordAlg
{
    private readonly string _password;
    private readonly byte[] _passwordByteArray;

    protected AbstractPasswordAlg(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
        }
        _password = password;
        _passwordByteArray = Encoding.UTF8.GetBytes(password);
    }

    public abstract string Encrypt(string decryptedValue);
    public abstract string Decrypt(string encryptedValue);

    protected string GetPassword()
    {
        return _password;
    }
    protected byte[] GetPasswordByteArray()
    {
        return _passwordByteArray;
    }
}
