namespace CFG2.Utils.SysLib;

using System.Runtime.InteropServices;

public class SysUtils
{
    private static readonly Dictionary<SpecialFolder, Guid> _guids = new()
    {
        [SpecialFolder.Contacts] = new("56784854-C6CB-462B-8169-88E350ACB882"),
        [SpecialFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
        [SpecialFolder.Favorites] = new("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
        [SpecialFolder.Links] = new("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
        [SpecialFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
        [SpecialFolder.SavedSearches] = new("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
    };

    public static bool EnvVarExists(string var)
    {
        return Environment.GetEnvironmentVariables().Contains(var);
    }

    public static string GetEnvVar(string var)
    {
        if (string.IsNullOrEmpty(var)) {
            throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(var));
        }
        if (!EnvVarExists(var)) {
            throw new KeyNotFoundException($"Environment variable '{var}' does not exist.");
        }
        
        #pragma warning disable CS8603 // Possible null reference return, handled in error checking above
        return Environment.GetEnvironmentVariable(var);
        #pragma warning restore CS8603 // Possible null reference return.
    }

    public static void SetEnvVar(string var, string val)
    {
        Environment.SetEnvironmentVariable(var, val);
    }

    /// <summary>
    /// Returns true if a file is in use/locked
    /// https://stackoverflow.com/questions/10504647/deleting-files-in-use
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool IsFileInUse(string fullpath)
    {
        bool inUse = false;
        if (File.Exists(fullpath)) {
            FileInfo file = new FileInfo(fullpath);
            long size = -1;
            while (size != file.Length) {
                size = file.Length;
                Thread.Sleep(3000);
            }
            Thread.Sleep(2000);

            FileStream? stream = null;

            try {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                inUse = false;
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                inUse = true;
            } finally {
                if (stream != null) {
                    stream.Close();
                }
            }
        }

        return inUse;
    }

    public static void PrependTextToFile(string file, string text)
    {
        if (string.IsNullOrEmpty(file))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(file));
        }
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Text to prepend cannot be null or empty.", nameof(text));
        }

        string content = File.ReadAllText(file);
        File.WriteAllText(file, text + Environment.NewLine + content);
    }

    /// <summary>
    /// Cleans a string to be safe for use as a file name. Do NOT send full path or extension.
    /// </summary>
    /// <param name="str">The string to sanitize.</param>
    /// <param name="removeEmailPrefixes">If true, removes common email prefixes like "Fwd: " and "Re: "</param>
    /// <returns>A version of <paramref name="str"/> that is safe to use for a filename.</returns>
    public static string GetFileNameSafeString(string str, bool removeEmailPrefixes = true)
    {
        if (!string.IsNullOrEmpty(str))
        {
            if (removeEmailPrefixes)
            {
                bool continueLoop = true;
                while (continueLoop)
                {
                    continueLoop = false;
                    if (str.StartsWith("fwd: ", StringComparison.OrdinalIgnoreCase))
                    {
                        str = str.Substring(5);
                        continueLoop = true;
                    }
                    if (str.StartsWith("re: ", StringComparison.OrdinalIgnoreCase))
                    {
                        str = str.Substring(4);
                        continueLoop = true;
                    }
                }
            }
            str = str.Replace("#", "")
                    .Replace("’", "")
                    .Replace("&", "and")
                    .Replace("/", "-")
                    .Replace("\\", "-")
                    .Replace("?", "")
                    .Replace(":", "")
                    .Replace(";", "")
                    .Replace("*", "")
                    .Replace("!", "")
                    .Replace(">", "")
                    .Replace("<", "")
                    .Replace("|", "-")
                    .Replace("\"", "")
                    .Trim();

            // Keep filename length reasonable
            if (str.Length > 150)
                str = str.Substring(0, 150).Trim();
        }
        return str;
    }

    /// <summary>
    /// Sanitizes a folder path by removing invalid characters from each folder name in the path. 
    /// For example, "C:\Temp\Inva|id\Fol*der" becomes "C:\Temp\Inva_id\Fol_der".
    /// </summary>
    /// <param name="folderName">The folder path to sanitize.</param>
    /// <param name="directorySeparator">The directory separator character. Defaults to Windows path separator (backslash).</param>
    /// <returns>A sanitized version of <paramref name="folderName"/>.</returns>
    public static string SanitizeFolderPath(string folderName, char directorySeparator = '\\')
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var parts = folderName.Split(directorySeparator);
        var sanitizedParts = parts.Select(part =>
            string.Join("_", part.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim()
        );

        return Path.Combine(sanitizedParts.ToArray());
    }

    /// <summary>
    /// Returns true if the file content is different than the provided string content. 
    /// Does this by writing the string to a temp file and comparing to try and get around encoding/lineending issues.
    /// </summary>
    /// <param name="file">File who's content to compare.</param>
    /// <param name="content">String content to compare against file content.</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
    public static bool IsFileDifferentThanString(string file, string content)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("File does not exist.", file);
        }

        string tempFile = GetTempFile();
        System.Text.Encoding encoding;
        using (var reader = new StreamReader(file, detectEncodingFromByteOrderMarks: true))
        {
            reader.Peek(); // Force detection of encoding by reading at least one character (Peek will not consume)
            encoding = reader.CurrentEncoding;
        }
        Console.WriteLine("Detected encoding for comparison: " + encoding);
        File.WriteAllText(tempFile, content, encoding);
        bool result = IsFileDifferent(file, tempFile);
        File.Delete(tempFile);

        return result;
    }

    /// <summary>
    /// Returns true if the two files are different, comparing line by line.
    /// </summary>
    /// <param name="file1">First file to compare</param>
    /// <param name="file2">Second file to compare</param>
    /// <returns>True if files are different, false if identical</returns>
    /// <exception cref="FileNotFoundException">If either file does not exist</exception>
    public static bool IsFileDifferent(string file1, string file2)
    {
        if (!File.Exists(file1) || !File.Exists(file2))
        {
            throw new FileNotFoundException("One or both files do not exist.");
        }

        using (var reader1 = new StreamReader(file1))
        using (var reader2 = new StreamReader(file2))
        {
            int lineNumber = 0;
            string? line1, line2;

            // Read until we hit end of one or both files
            while ((line1 = reader1.ReadLine()) != null)
            {
                lineNumber++;
                line2 = reader2.ReadLine();

                if (line2 == null)
                {
                    Console.WriteLine($"Files differ at line {lineNumber}: Second file is shorter");
                    return true;
                }

                if (line1 != line2)  // Note: this does exact string comparison
                {
                    Console.WriteLine($"There is a difference on line {lineNumber}:");
                    return true;
                }
            }

            // Check if second file is longer
            if (reader2.ReadLine() != null)
            {
                Console.WriteLine($"Files differ at line {lineNumber + 1}: First file is shorter");
                return true;
            }
        }

        return false;
    }

    public static string GetTempFile()
    {
        return Path.GetTempFileName();
    }

    public static string GetSpecialFolder(SpecialFolder specialFolder)
    {
        string ret;
        if (specialFolder == SpecialFolder.AppData)
        {
            ret = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (specialFolder == SpecialFolder.Temp)
        {
            ret = Path.GetTempPath();
        }
        else
        {
            ret = SHGetKnownFolderPath(_guids[specialFolder], 0);
        }

        if ((ret == null) || !Directory.Exists(ret))
        {
            throw new Exception("Failed to get special folder: " + specialFolder);
        }

        return ret;
    }

    [DllImport("shell32",
        CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
        nint hToken = 0);
}