namespace CFG2.Utils.SysLib;

public class MiscLib
{
    public static string GenerateCorrelationId(string prefix = "")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            prefix = "CID";
        }
        return prefix.ToUpper() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
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