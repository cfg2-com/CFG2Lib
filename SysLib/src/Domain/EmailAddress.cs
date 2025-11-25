namespace CFG2.Utils.SysLib.Domain;

public class EmailAddress
{
    private readonly string _originalAddress;
    private readonly string _emailAddress;
    private readonly bool _isValid;
    private readonly bool _isCleansed;
    private readonly string _userPart;
    private readonly string _domainPart;
    private readonly string _tldPart;

    public string Address => _emailAddress;
    public bool IsValid => _isValid;
    public bool IsCleansed => _isCleansed;
    public string User => _userPart;
    public string Domain => _domainPart;
    public string TLD => _tldPart;

    public EmailAddress(string address)
    {
        _originalAddress = address;
        _emailAddress = _originalAddress.Trim().ToLower();
        _isValid = IsValidEmail(_emailAddress);
        _isCleansed = !(_emailAddress == _originalAddress);

        if (_isValid)
        {
            var parts = _emailAddress.Split('@');
            _userPart = parts[0];
            _domainPart = parts[1];
            _tldPart = _domainPart.Split('.').Last();
        }
        else
        {
            _userPart = "";
            _domainPart = "";
            _tldPart = "";
        }
    }

    public static bool IsValidEmail(string address)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(address,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}