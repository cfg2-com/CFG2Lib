using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CFG2.Utils.LogLib;

// Src: https://csharpindepth.com/articles/Singleton
public sealed class Logger {
    private static readonly Dictionary<string, Logger> _instances = [];
    private static readonly object _padlock = new();

    private readonly string _logFilename = "";
    private readonly string _logDir = "";
    private readonly string _logFile = "";

    private Logger(string dir, string filename, int retentionDays = 30)
    {
        _logDir = dir;

        if (!Directory.Exists(_logDir))
        {
            Trace("Log directory does not exist, creating: " + _logDir);
            Directory.CreateDirectory(_logDir);
        }

        _logFilename = filename;
        _logFile = Path.Combine(_logDir, _logFilename);

        CleanupOldLogs(retentionDays);
    }

    /// <summary>
    /// Returns a singleton instance of Logger for the specified application
    /// </summary>
    /// <param name="appDir">Base directory of the application. Logs will be created within a "Logs" directory under this path. If the path does not exist an Exception will be thrown.</param>
    /// <param name="retentionDays">Number of days to retain log entries. Entries older than this will be removed when the Logger is instantiated. Default is 30 days.</param>
    /// <param name="appName">If null is provided this will default to whatever is returned by Process.GetCurrentProcess().ProcessName</param>
    /// <param name="filename">If null the Logger will log to {appDir}/Logs/{appName}.log</param>
    /// <returns></returns>
    public static Logger Instance(string appDir, int retentionDays = 30, string? appName = null, string? filename = null)
    {
        if (!Directory.Exists(appDir)){
            throw new DirectoryNotFoundException("The specified application directory does not exist: " + appDir);
        }

        if (appName == null)
        {
            appName = Process.GetCurrentProcess().ProcessName;
        }
        if (filename == null)
        {
            filename = appName + ".log";
        }
        lock (_padlock)
        {
            if (!_instances.ContainsKey(filename))
            {
                string logDir = Path.Combine(appDir, "Logs");
                _instances.Add(filename, new Logger(logDir, filename, retentionDays));
            }
            return _instances[filename];
        }
    }

    /// <summary>
    /// Returns the filename of the log file
    /// </summary>
    /// <returns>Just the file name (no path info)</returns>
    public string GetFilename()
    {
        return _logFilename;
    }

    /// <summary>
    /// Returns the directory where the log file is stored
    /// </summary>
    /// <returns>Log file directory</returns>
    public string GetDir()
    {
        return _logDir;
    }

    /// <summary>
    /// Returns the full path to the log file
    /// </summary>
    /// <returns>Full path to log file</returns>
    public string GetFile()
    {
        return _logFile;
    }
    
    /// <summary>
    /// Writes just msg to the console (no timestamp or level)
    /// </summary>
    /// <param name="msg"></param>
    public static void Trace(string msg)
    {
        // This is a static method to allow for easy tracing without needing an instance
        Console.WriteLine(msg);
    }

    /// <summary>
    /// Writes msg to the log file and console
    /// </summary>
    /// <param name="msg"></param>
    public void Debug(string msg) {
        LogIt("DEBUG : "+msg);
    }

    /// <summary>
    /// Writes msg to the log file and console
    /// </summary>
    /// <param name="msg"></param>
    public void Log(string msg) {
        LogIt("INFO  : "+msg);
    }

    /// <summary>
    /// Writes msg to the log file and console
    /// </summary>
    /// <param name="msg"></param>
    public void Warn(string msg) {
        LogIt("WARN  : "+msg);
    }

    /// <summary>
    /// Writes msg to the log file and console
    /// </summary>
    /// <param name="msg"></param>
    public void Error(string msg) {
        LogIt("ERROR : "+msg);
    }

    private void LogIt(string msg)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(_logFile))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + msg);
            }
        }
        catch (Exception e)
        {
            Trace("Exception in LoggerLib: " + e.Message);
        }
        Trace(msg); // Console.WriteLine
    }
    
    private void CleanupOldLogs(int retentionDays)
    {
        try
        {
            if (!File.Exists(_logFile)) {
                return;
            }

            string tempFile = Path.GetTempFileName();
            DateTime threshold = DateTime.Now.AddDays(-retentionDays);
            bool foundRecent = false;

            using (var sr = new StreamReader(_logFile))
            using (var sw = new StreamWriter(tempFile, false, Encoding.UTF8))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!foundRecent)
                    {
                        if (line.Length >= 19)
                        {
                            string tsPart = line.Substring(0, 19);
                            if (DateTime.TryParseExact(tsPart, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ts))
                            {
                                if (ts >= threshold)
                                {
                                    // This and all following lines are recent enough — keep them
                                    foundRecent = true;
                                    Trace("Cleaning up old log entires prior to: " + threshold.ToString("yyyy-MM-dd HH:mm:ss"));
                                    sw.WriteLine(line);
                                }
                                // else skip this old line
                            }
                            // else line doesn't start with a timestamp — skip until we find a timestamp within range
                        }
                        // else line too short to contain timestamp — skip
                    }
                    else
                    {
                        sw.WriteLine(line);
                    }
                }
            }

            if (foundRecent)
            {
                // Replace original file with temp file
                File.Copy(tempFile, _logFile, true);
                File.Delete(tempFile);
            }
            else
            {
                // No recent entries found — remove original log and temp file
                File.Delete(_logFile);
                File.Delete(tempFile);
            }
        }
        catch (Exception e)
        {
            Trace("Exception during log cleanup: " + e.Message);
        }
    }
}