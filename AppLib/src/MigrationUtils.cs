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
                Logger.Trace("AppConfig moved successfully to " + appConfig.GetFile());
                success = true;
            }
        }

        if (success)
        {
            appConfig.Reload();
        }
        return success;
    }
    
    public static bool MigrateFile(string legacyFile, string newFile)
    {
        bool success = false;
        if (File.Exists(newFile))
        {
            Logger.Trace("Skipping migration (but returning success) because newFile already exists: " + newFile);
            success = true;
        }
        else if (File.Exists(legacyFile))
        {
            Logger.Trace($"Moving '{legacyFile}' to '{newFile}'");
            File.Move(legacyFile, newFile);
            if (!File.Exists(legacyFile) && File.Exists(newFile))
            {
                Logger.Trace("legacyFile moved successfully to "+newFile);
                success = true;
            }
        }
        return success;
    }
    
    public static bool MigrateDeduper(string legacyFile, Deduper deduper)
    {
        bool success = false;
        if (!deduper.UseMDP && File.Exists(deduper.GetFile()))
        {
            Logger.Trace("Skipping migration (but returning success) because Deduper file already exists: " + deduper.GetFile());
            success = true;
        }
        else if (File.Exists(legacyFile))
        {
            Logger.Trace($"Moving '{legacyFile}' to '{deduper.GetFile()}'");

            if (deduper.UseMDP)
            {
                using (StreamReader sr = File.OpenText(legacyFile))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line) && !line.Trim().StartsWith("#") && (line.Trim().Length != 0))
                        {
                            deduper.AddItem(line, "From MigrationUtils.MigrateDeduper()");
                        }
                    }
                }
                File.Delete(legacyFile);
            }
            else
            {
                File.Move(legacyFile, deduper.GetFile());
            }

            if (!File.Exists(legacyFile) && File.Exists(deduper.GetFile()))
            {
                Logger.Trace("legacyFile moved successfully to " + deduper.GetFile());
                success = true;
            }
        }

        if (success)
        {
            deduper.Reload();
        }
        return success;
    }
    
}