namespace CFG2.Utils.SysLib;

using System.Runtime.InteropServices;

public class SysLib
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
        bool ret = false;
        if (GetEnvVar(var) != null) {
            ret = true;
        }
        return ret;
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

    public static string GetSpecialFolder(SpecialFolder specialFolder)
    {
        string ret;
        if (specialFolder == SpecialFolder.AppData) {
            ret = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        } else {
            ret = SHGetKnownFolderPath(_guids[specialFolder], 0);
        }

        if ((ret == null) || !Directory.Exists(ret)) {
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
