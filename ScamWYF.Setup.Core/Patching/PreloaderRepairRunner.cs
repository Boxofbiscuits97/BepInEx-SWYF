using System;
using ScamWYF.Setup.Patching;

namespace ScamWYF.Setup
{
    public static class PreloaderRepairRunner
    {
        public static bool Run(string managedFolder, string overrideFolder, ISetupLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (!System.IO.Directory.Exists(managedFolder))
            {
                log.Error("Automatic repairs skipped: game Managed folder was not found: " + managedFolder);
                return false;
            }

            if (!System.IO.Directory.Exists(overrideFolder))
            {
                log.Error("Automatic repairs skipped: Doorstop assembly override folder was not found: " + overrideFolder);
                return false;
            }

            log.Heading("restoring stripped awaiter methods");
            new AwaiterUnstripper().Run(managedFolder, AwaiterUnstripper.DefaultAssemblies, overrideFolder, log);

            log.Heading("patching remaining vtable breakages");
            var fixer = new VtableBreakFixer();
            fixer.Run(managedFolder, true, null, overrideFolder, log);

            log.Heading("verifying vtable repairs");
            var verification = fixer.Run(managedFolder, false, null, overrideFolder, log);
            if (!verification.Clean)
            {
                log.Error("Automatic vtable repairs left " + verification.Breakages + " breakage(s).");
            }

            return verification.Clean;
        }
    }
}
