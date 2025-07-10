using System.Diagnostics;

namespace CFG2.Utils.LogLib;

// Src: https://csharpindepth.com/articles/Singleton
public sealed class Logger {
    private static readonly Dictionary<string, Logger> instances = [];
    private static readonly object padlock = new();

    private readonly string logFilename = "";
    private readonly string logDir = "";
    private readonly string logFile = "";

    private Logger(string dir, string filename) {
        logDir = dir;

        if (!Directory.Exists(logDir)) {
            Trace("Log directory does not exist, creating: " + logDir);
            Directory.CreateDirectory(logDir);
        }

        logFilename = filename;
        logFile = Path.Combine(logDir, logFilename);
    }

    /// <summary>
    /// Returns a singleton instance of Logger for the specified application
    /// </summary>
    /// <param name="appDir">Base directory of the application. Logs will be created within a "Logs" directory under this path. If the path does not exist an Exception will be thrown.</param>
    /// <param name="appName">If null is provided this will default to whatever is returned by Process.GetCurrentProcess().ProcessName</param>
    /// <param name="filename">If null the Logger will log to {appDir}/Logs/{appName}.log</param>
    /// <returns></returns>
    public static Logger Instance(string appDir, string? appName = null, string? filename = null)
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
        lock (padlock)
        {
            if (!instances.ContainsKey(filename))
            {
                string logDir = Path.Combine(appDir, "Logs");
                instances.Add(filename, new Logger(logDir, filename));
            }
            return instances[filename];
        }
    }

    /// <summary>
    /// Returns the filename of the log file
    /// </summary>
    /// <returns>Just the file name (no path info)</returns>
    public string GetFilename()
    {
        return logFilename;
    }

    /// <summary>
    /// Returns the directory where the log file is stored
    /// </summary>
    /// <returns>Log file directory</returns>
    public string GetDir()
    {
        return logDir;
    }

    /// <summary>
    /// Returns the full path to the log file
    /// </summary>
    /// <returns>Full path to log file</returns>
    public string GetFile()
    {
        return logFile;
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

    private void LogIt(string msg) {
        try {
            using (StreamWriter sw = File.AppendText(logFile)) {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" : "+msg);
            }
        } catch (Exception e) {
            Trace("Exception in LoggerLib: "+e.Message);
        }
        Trace(msg); // Console.WriteLine
    }
}