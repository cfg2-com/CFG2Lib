namespace CFG2.Utils.AppLib;

using System.Diagnostics;
using CFG2.Utils.SysLib;
using CFG2.Utils.LogLib;
using CFG2.Utils.SQLiteLib;

public class App
{
    private static Logger _logger;
    private readonly string _configRootDir;
    private readonly string _appName;
    private readonly string _baseDir;
    private readonly int _retentionDays;
    private SQLiteUtil _mdp;


    public App(string? appName = null, bool configInSyncDir = true, string? baseDir = null, int retentionDays = 30)
    {
        _retentionDays = retentionDays;

        // Make sure we have an AppName
        if (string.IsNullOrEmpty(appName))
        {
            _appName = Process.GetCurrentProcess().ProcessName;
            Logger.Trace("Using Process.GetCurrentProcess().ProcessName for appName: " + _appName);
        }
        else
        {
            Logger.Trace("Using provided appName: " + appName);
            _appName = appName;
        }

        // Make sure we have a baseDir
        if (string.IsNullOrEmpty(baseDir))
        {
            Logger.Trace("Using default baseDir: CFG2");
            _baseDir = "CFG2";
        }
        else
        {
            Logger.Trace("Using provided baseDir: " + baseDir);
            _baseDir = baseDir;
        }

        // Intentionally calling this here because expected for all apps regardless of configInSyncDir setting.
        // Basically, if SYNC_DRIVE_HOME is set, then make sure it actually exists (if not set, will default to AppData).
        // MUST come after appName and baseDir are set, because GetSyncDir() MIGHT use them (when SYNC_DRIVE_HOME isn't set).
        string syncHome = GetSyncDir();

        // Get (and create if necessary) the configRootDir: {syncOrAppDir}/{baseDir}.
        if (configInSyncDir)
        {
            string dir = Path.Combine(syncHome, "Apps");
            if (!Directory.Exists(dir))
            {
                Logger.Trace("Creating: " + dir);
                Directory.CreateDirectory(dir);
            }
            _configRootDir = Path.Combine(dir, _baseDir);
        }
        else
        {
            _configRootDir = Path.Combine(SysLib.GetSpecialFolder(SpecialFolder.AppData), _baseDir);
        }
        Logger.Trace("ConfigRootDir: " + _configRootDir);
        if (!Directory.Exists(_configRootDir))
        {
            Directory.CreateDirectory(_configRootDir);
        }

        // Get (and create if necessary) the app directory: {configRootDir}/{appName}.
        // While at the same time initializing the logger instance.
        _logger = Logger.Instance(GetAppDir());

        CleanupSoftDeleteDirectory(_retentionDays);
    }

    public string Name => GetAppName();
    public string Dir => GetAppDir();
    public string DataDir => GetAppDataDir();
    public string LogDir => GetAppLogDir();
    public string SoftDeleteDir => GetAppSoftDeleteDir();
    public string BackupDir => GetAppBackupDir();
    public string BackupBaseDir => GetBackupBaseDir();
    public string BackupRootDir => GetBackupRootDir();
    public string LogFile => _logger.GetFile();
    public string SyncDir => GetSyncDir();
    public string InboxDir => GetInboxDir();

    private string GetAppName()
    {
        return _appName;
    }

    private string GetAppDir()
    {
        string dir = Path.Combine(_configRootDir, _appName);
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating AppDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    private string GetAppDataDir()
    {
        string dir = Path.Combine(GetAppDir(), "Data");
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating AppDataDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    private string GetAppLogDir()
    {
        string dir = Path.Combine(GetAppDir(), "Logs");
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating AppLogDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    /// <summary>
    /// This is always in the sync dir
    /// </summary>
    /// <returns>[syncDir]/Backup/[baseDir]/[appName]</returns>
    private string GetAppBackupDir()
    {
        string dir = Path.Combine(GetBackupBaseDir(), _appName);
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating AppBackupDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    private string GetAppSoftDeleteDir()
    {
        string dir = Path.Combine(BackupDir, "Delete");
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating AppSoftDeleteDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    /// <summary>
    /// Returns the sync directory. Does not matter if configInSyncDir is true or false, or what the app directory/name is. 
    /// Usually this will be the root of a syncronized directory (e.g. Google Drive, OneDrive, etc.), and that root should be 
    /// set in the SYNC_DRIVE_HOME environment variable. If the enviroment variable does not exist, it will default to the 
    /// system AppData directory.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GetSyncDir()
    {
        string syncHome;
        if (!SysLib.EnvVarExists("SYNC_DRIVE_HOME"))
        {
            syncHome = Path.Combine(SysLib.GetSpecialFolder(SpecialFolder.AppData), _baseDir);
            if (!Directory.Exists(syncHome))
            {
                Logger.Trace("Creating: " + syncHome);
                Directory.CreateDirectory(syncHome);
            }
            Logger.Trace("WARN: SYNC_DRIVE_HOME does not exist. Using: " + syncHome);
        }
        else
        {
            //Logger.Trace("Using SYNC_DRIVE_HOME environment variable");
            syncHome = SysLib.GetEnvVar("SYNC_DRIVE_HOME");
        }


        if (!Directory.Exists(syncHome))
        {
            throw new Exception("SyncDir does not exist: " + syncHome);
        }
        return syncHome;
    }

    private string GetInboxDir()
    {
        string dir = Path.Combine(GetSyncDir(), "_Inbox");
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating InboxDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    private string GetBackupRootDir()
    {
        string dir = Path.Combine(GetSyncDir(), "Backup");
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating BackupRootDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    private string GetBackupBaseDir()
    {
        string dir = Path.Combine(GetBackupRootDir(), _baseDir);
        if (!Directory.Exists(dir)) {
            Logger.Trace("Creating BackupBaseDir: " + dir);
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    public void Trace(string msg)
    {
        Logger.Trace(msg);
    }

    public void Log(string msg)
    {
        _logger.Log(msg);
    }

    public void Warn(string msg)
    {
        _logger.Warn(msg);
    }

    public void Error(string msg)
    {
        _logger.Error(msg);
    }

    public SQLiteUtil GetMDP()
    {
        if (_mdp == null)
        {
            string mdpFile = Path.Combine(this.SyncDir, "MDP.db");
            _mdp = new SQLiteUtil(mdpFile);
        }

        return _mdp;
    }

    public bool SoftDeleteFile(string fullpath)
    {
        bool success = false;
        if (File.Exists(fullpath))
        {
            string deleteDir = GetAppSoftDeleteDir();
            try
            {
                _logger.Log("Soft deleting: " + fullpath);
                string dstFile = Path.Combine(deleteDir, Path.GetFileName(fullpath));
                int count = 0;
                string fileWoExt = Path.GetFileNameWithoutExtension(fullpath);
                string fileExt = Path.GetExtension(fullpath);
                while (File.Exists(dstFile)) // Create a unique filename
                {
                    count++;
                    dstFile = Path.Combine(deleteDir, fileWoExt + " " + count + fileExt);
                }
                File.Copy(fullpath, dstFile, true); // Using copy then delete because sometimes "Move" is weird when dealing with cloud drives
                if (!File.Exists(dstFile))
                {
                    _logger.Error("Soft delete failed copying " + fullpath + " to " + dstFile);
                }
                else
                {
                    File.Delete(fullpath); // Delete the original file
                    if (File.Exists(fullpath))
                    {
                        _logger.Error("Soft delete file still exists: " + fullpath);
                    }
                    else
                    {
                        _logger.Log("Soft deleted to: " + dstFile);
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Trying to soft delete: " + fullpath + " : " + e.Message);
            }
        }
        else
        {
            _logger.Warn("File does not exist to soft delete: " + fullpath);
            success = true; // If the source file doesn't exist, we consider it a success
        }

        return success;
    }

    public void CleanupSoftDeleteDirectory(int days)
    {
        string deleteDir = GetAppSoftDeleteDir();
        if (Directory.Exists(deleteDir)) {
            DateTime RetentionThreshold = DateTime.Now.AddDays(days * -1);
            string[] files = Directory.GetFiles(deleteDir);
            foreach (string file in files) {
                if (File.GetLastWriteTime(file).CompareTo(RetentionThreshold) < 0) {
                    _logger.Log("Deleting: "+file);
                    File.Delete(file);
                }
            }
        }
    }
}
