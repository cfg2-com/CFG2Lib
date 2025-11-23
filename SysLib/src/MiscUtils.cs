namespace CFG2.Utils.SysLib;

public class MiscUtils
{
    private static readonly object _lock = new();
    private static string _lastTimestamp = "";
    private static int _counter;

    /// <summary>
    /// Generate a unique Correlation ID based on current timestamp and specified prefix (default to CID). 
    /// This is thread-safe and will increment a counter if multiple IDs are generated within the same minute. 
    /// Not inteded for all scenarious and complete uniqueness, but should be sufficient for most logging/tracking purposes.
    /// </summary>
    /// <param name="prefix">Prefix for returned value. Will be capitalized.</param>
    /// <returns>A value in the format: PREFIX-0000</returns>
    public static string GenerateCorrelationId(string prefix = "CID")
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "CID";
            }
            string currentTimestamp = DateTime.Now.ToString("yyMMddHHmm");
            if (currentTimestamp == _lastTimestamp)
            {
                _counter++;
            }
            else
            {
                _lastTimestamp = currentTimestamp;
                _counter = 0;
            }
            return _counter > 0
                ? $"{prefix.ToUpper()}-{currentTimestamp}{_counter}"
                : $"{prefix.ToUpper()}-{currentTimestamp}";
        }
    }

    /// <summary>
    /// Extract Correlation ID from subject and/or detail. Will return the first one found in the subject, then detail. 
    /// Subject will be checked 1st for " -> CORRID" then " (COR-ID)" pattern at end of string. 
    /// Detail will be checked for "correlation: CORRID" line.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="detail"></param>
    /// <param name="arrowCheck">Enable/disable arrow pattern " -> CORRID" check in subject.</param>
    /// <param name="parenCheck">Enable/disable parenthesis pattern " (COR-ID)" check in subject.</param>
    /// <param name="detailKeyword">Keyword to search for in detail lines.</param>
    /// <returns>A Correlation ID if found, otherwise empty string.</returns>
    public static string GetCorrelationId(string subject, string detail = "", bool arrowCheck = true, bool parenCheck = true, string detailKeyword = "correlation: ")
    {
        if (!string.IsNullOrEmpty(subject))
        {
            if (arrowCheck)
            {
                int startIdx = subject.LastIndexOf(" -> ");
                if (startIdx >= 0)
                {
                    int endIdx = subject.Length;
                    if (endIdx > startIdx)
                    {
                        return subject.Substring(startIdx + 4).Trim();
                    }
                }
            }

            if (parenCheck)
            {
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(subject, @"\s\((\w+-\d+)\)$");
                if (match.Success)
                {
                    string matchStr = match.Groups[1].Value;
                    return matchStr.Replace("(", "").Replace(")", "").Trim();
                }
            }
        }

        if (!string.IsNullOrEmpty(detail) && !string.IsNullOrEmpty(detailKeyword))
        {
            string[] lines = detail.Split('\n');
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith(detailKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Replace(detailKeyword, "").Trim();
                }
            }
        }

        return "";
    }
}