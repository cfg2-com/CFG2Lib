namespace CFG2.Utils.SysLib.Domain;

/// <summary>
/// Represents a phone number, providing parsing, validation, and formatting.
/// Assumes North American Numbering Plan (NANP) for 10-digit numbers.
/// </summary>
public class PhoneNumber
{
    private readonly string _originalNumber;
    private readonly string _cleansedNumber;

    public string OriginalNumber => _originalNumber;
    public string E164Format { get; }
    public bool IsValid => _isValid;
    public string CountryCode { get; }
    public string AreaCode { get; }
    public string Exchange { get; }
    public string LineNumber { get; }
    public string FormattedNumberI18N { get; }
    public string FormattedNumberUS { get; }
    public string FormattedNumberUSparens { get; }
    public string FormattedNumberUSdots { get; }

    private readonly bool _isValid;

    public PhoneNumber(string number)
    {
        _originalNumber = number ?? string.Empty;

        // Initialize properties to empty strings
        CountryCode = AreaCode = Exchange = LineNumber = E164Format = FormattedNumberI18N = FormattedNumberUS = FormattedNumberUSparens = FormattedNumberUSdots = string.Empty;

        string numberToParse = CleanNumber(_originalNumber);
        _cleansedNumber = numberToParse.StartsWith("+") ? numberToParse.Substring(1) : numberToParse;

        _isValid = IsValidPhoneNumber(numberToParse);

        if (!_isValid)
        {
            return;
        }

        string digitsOnly = _cleansedNumber;

        // Assume US/NANP number if 10 digits and add country code
        if (digitsOnly.Length == 10 && !numberToParse.StartsWith("+"))
        {
            digitsOnly = "1" + digitsOnly;
        }
        E164Format = "+" + digitsOnly.TrimStart('+');

        if (_isValid)
        {
            // Assuming NANP format for numbers that are 11 digits and start with '1'
            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("1"))
            {
                CountryCode = digitsOnly.Substring(0, 1);
                AreaCode = digitsOnly.Substring(1, 3);
                Exchange = digitsOnly.Substring(4, 3);
                LineNumber = digitsOnly.Substring(7, 4);
                FormattedNumberI18N = $"+{CountryCode} ({AreaCode}) {Exchange}-{LineNumber}";
                FormattedNumberUS = $"{AreaCode}-{Exchange}-{LineNumber}";
                FormattedNumberUSparens = $"({AreaCode}) {Exchange}-{LineNumber}";
                FormattedNumberUSdots = $"{AreaCode}.{Exchange}.{LineNumber}";
            }
            else // Handle other international numbers
            {
                // Basic international parsing, mileage may vary.
                // E.g. UK numbers can have variable length parts.
                // This is a simple case.
                CountryCode = digitsOnly.Substring(0, digitsOnly.Length - 10);

                // The rest is considered the local number
                var localNumber = digitsOnly.Substring(CountryCode.Length);
                FormattedNumberI18N = $"+{CountryCode} {localNumber}";

                // For non-NANP numbers, we'll leave other properties empty
                // as we can't reliably parse them without a proper library.
                AreaCode = string.Empty;
                Exchange = string.Empty;
                LineNumber = string.Empty;
            }
        }
    }

    private static string CleanNumber(string number)
    {
        if (string.IsNullOrEmpty(number)) return string.Empty;
        string digits = System.Text.RegularExpressions.Regex.Replace(number, @"[^\d]", "");
        return number.Trim().StartsWith("+") ? "+" + digits : digits;
    }

    public static bool IsValidPhoneNumber(string number)
    {
        // Basic E.164-like validation. Optional '+', then 7 to 15 digits.
        return System.Text.RegularExpressions.Regex.IsMatch(number, @"^\+?[1-9]\d{6,14}$");
    }
}