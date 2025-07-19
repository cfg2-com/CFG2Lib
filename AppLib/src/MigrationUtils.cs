using CFG2.Utils.LogLib;

namespace CFG2.Utils.AppLib;

public class MigrationUtils
{
    public static bool MigrateAppConfigFile(string legacyFile, AppConfig appConfig)
    {
        bool success = false;
        if (File.Exists(appConfig.GetFile()))
        {
            Logger.Trace("Skipping migration (but returning success) because AppConfig file already exists: " + appConfig.GetFile());
            success = true;
        }
        else if (File.Exists(legacyFile))
        {
            Logger.Trace($"Moving '{legacyFile}' to '{appConfig.GetFile()}'");
            File.Move(legacyFile, appConfig.GetFile());
            if (!File.Exists(legacyFile) && File.Exists(appConfig.GetFile()))
            {
                Logger.Trace("AppConfig moved successfully to "+appConfig.GetFile());
                success = true;
            }
        }

        if (success)
        {
            appConfig.Reload();
        }
        return success;
    }
}