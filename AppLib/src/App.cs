namespace CFG2.Utils.AppLib;

using System.Diagnostics;
using CFG2.Utils.SysLib;
using CFG2.Utils.LogLib;
using CFG2.Utils.SQLiteLib;

public class App
{
    private static Logger logger;
    private readonly string configRootDir;
    private readonly string appName;
    private readonly string baseDir;
    private readonly int retentionDays;
    private SQLiteUtil mdp;


    public App(string? appName = null, bool configInSyncDir = true, string? baseDir = null, int retentionDays = 30)
    {
        this.retentionDays = retentionDays;

        // Make sure we have an AppName
        if (string.IsNullOrEmpty(appName))
        {
            this.appName = Process.GetCurrentProcess().ProcessName;
            Logger.Trace("Using Process.GetCurrentProcess().ProcessName for appName: " + this.appName);
        }
        else
        {
            Logger.Trace("Using provided appName: " + appName);
            this.appName = appName;
        }

        // Make sure we have a baseDir
        if (string.IsNullOrEmpty(baseDir))
        {
            Logger.Trace("Using default baseDir: CFG2");
            this.baseDir = "CFG2";
        }
        else
        {
            Logger.Trace("Using provided baseDir: " + baseDir);
            this.baseDir = baseDir;
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
            this.configRootDir = Path.Combine(dir, this.baseDir);
        }
        else
        {
            this.configRootDir = Path.Combine(SysLib.GetSpecialFolder(SpecialFolder.AppData), this.baseDir);
        }
        Logger.Trace("ConfigRootDir: " + this.configRootDir);
        if (!Directory.Exists(this.configRootDir))
        {
            Directory.CreateDirectory(this.configRootDir);
        }

        // Get (and create if necessary) the app directory: {configRootDir}/{appName}.
        // While at the same time initializing the logger instance.
        logger = Logger.Instance(GetAppDir());

        CleanupSoftDeleteDirectory(this.retentionDays);
    }

    public string Name => GetAppName();
    public string Dir => GetAppDir();
    public string DataDir => GetAppDataDir();
    public string LogDir => GetAppLogDir();
    public string SoftDeleteDir => GetAppSoftDeleteDir();
    public string BackupDir => GetAppBackupDir();
    public string BackupBaseDir => GetBackupBaseDir();
    public string BackupRootDir => GetBackupRootDir();
    public string LogFile => logger.GetFile();
    public string SyncDir => GetSyncDir();
    public string InboxDir => GetInboxDir();

    private string GetAppName()
    {
        return appName;
    }

    private string GetAppDir()
    {
        string dir = Path.Combine(this.configRootDir, this.appName);
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
        string dir = Path.Combine(GetBackupBaseDir(), appName);
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
            syncHome = Path.Combine(SysLib.GetSpecialFolder(SpecialFolder.AppData), this.baseDir);
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
        string dir = Path.Combine(GetBackupRootDir(), this.baseDir);
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
        logger.Log(msg);
    }

    public void Warn(string msg)
    {
        logger.Warn(msg);
    }

    public void Error(string msg)
    {
        logger.Error(msg);
    }

    public SQLiteUtil GetMDP()
    {
        if (this.mdp == null)
        {
            string mdpFile = Path.Combine(this.SyncDir, "MDP.db");
            this.mdp = new SQLiteUtil(mdpFile);
        }

        return this.mdp;
    }

    public bool SoftDeleteFile(string fullpath)
    {
        bool success = false;
        if (File.Exists(fullpath))
        {
            string deleteDir = GetAppSoftDeleteDir();
            try
            {
                logger.Log("Soft Deleting: " + fullpath);
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
                    logger.Error("Failed copying " + fullpath + " to " + dstFile);
                }
                else
                {
                    File.Delete(fullpath); // Delete the original file
                    if (File.Exists(fullpath))
                    {
                        logger.Error("Failed deleting " + fullpath);
                    }
                    else
                    {
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Trying to delete: " + fullpath + " : " + e.Message);
            }
        }
        else
        {
            logger.Warn("File does not exist to Delete: " + fullpath);
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
                    logger.Log("Deleting: "+file);
                    File.Delete(file);
                }
            }
        }
    }
}
