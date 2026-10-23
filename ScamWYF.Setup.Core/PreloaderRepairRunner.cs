using BepInEx.Logging;

namespace ScamWYF.Setup
{
    public static class PreloaderRepairRunner
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("SWYF.PreloaderRepairRunner");

        public static bool Run(string managedFolder, string overrideFolder)
        {
            if (!System.IO.Directory.Exists(managedFolder))
            {
                Log.LogError("Automatic repairs skipped: game Managed folder was not found: " + managedFolder);
                return false;
            }

            if (!System.IO.Directory.Exists(overrideFolder))
            {
                Log.LogError("Automatic repairs skipped: Doorstop assembly override folder was not found: " + overrideFolder);
                return false;
            }

            Log.LogInfo("=== restoring stripped awaiter methods ===");
            new AwaiterUnstripper().Run(managedFolder, AwaiterUnstripper.DefaultAssemblies, overrideFolder);

            Log.LogInfo("=== patching remaining vtable breakages ===");
            var fixer = new VtableBreakFixer();
            fixer.Run(managedFolder, true, null, overrideFolder);

            Log.LogInfo("=== verifying vtable repairs ===");
            var verification = fixer.Run(managedFolder, false, null, overrideFolder);
            if (!verification.Clean)
            {
                Log.LogError("Automatic vtable repairs left " + verification.Breakages + " breakage(s).");
            }

            return verification.Clean;
        }
    }
}
