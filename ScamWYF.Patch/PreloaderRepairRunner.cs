using System;
using System.IO;
using ScamWYF.Setup.Patching;

namespace ScamWYF.Setup
{
    public sealed class GameInstall
    {
        public GameInstall(string managedFolder, string overrideFolder)
        {
            ManagedFolder = managedFolder;
            OverrideFolder = overrideFolder;
        }

        public string ManagedFolder { get; private set; }
        public string OverrideFolder { get; private set; }
    }

    public sealed class SetupException : Exception
    {
        public SetupException(string message) : base(message) { }
    }

    public static class PreloaderRepairRunner
    {
        public static bool Run(string managedFolder, string overrideFolder, ISetupLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (!Directory.Exists(managedFolder))
            {
                log.Error("Automatic repairs skipped: game Managed folder was not found: " + managedFolder);
                return false;
            }

            if (!Directory.Exists(overrideFolder))
            {
                log.Error("Automatic repairs skipped: Doorstop assembly override folder was not found: " + overrideFolder);
                return false;
            }

            var game = new GameInstall(managedFolder, overrideFolder);

            log.Heading("restoring stripped awaiter methods");
            new AwaiterUnstripper().Run(game, AwaiterUnstripper.DefaultAssemblies, overrideFolder, log);

            log.Heading("patching remaining vtable breakages");
            var fixer = new VtableBreakFixer();
            fixer.Run(game, true, null, overrideFolder, log);

            log.Heading("verifying vtable repairs");
            var verification = fixer.Run(game, false, null, overrideFolder, log);
            if (!verification.Clean)
            {
                log.Error("Automatic vtable repairs left " + verification.Breakages + " breakage(s).");
            }

            return verification.Clean;
        }
    }
}
