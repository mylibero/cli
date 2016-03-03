using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Build.Framework;
using Microsoft.Extensions.PlatformAbstractions;

using static Microsoft.DotNet.Cli.Build.Framework.BuildHelpers;

namespace Microsoft.DotNet.Cli.Build
{
    public static class PublishTargets
    {
        [Target]
        public static BuildTargetResult InitPublish(BuildTargetContext c)
        {
            return c.Success();
        }

        [Target(nameof(PrepareTargets.Init),
        nameof(PublishTargets.InitPublish),
        nameof(PublishTargets.PublishArtifacts))]
        public static BuildTargetResult Publish(BuildTargetContext c)
        {
            // NOTE(anurse): Currently, this just invokes the remaining build scripts as-is. We should port those to C# as well, but
            // I want to get the merged in.

            // Set up the environment variables previously defined by common.sh/ps1
            // This is overkill, but I want to cover all the variables used in all OSes (including where some have the same names)
            /*var buildVersion = c.BuildContext.Get<BuildVersion>("BuildVersion");
            var configuration = c.BuildContext.Get<string>("Configuration");
            var architecture = PlatformServices.Default.Runtime.RuntimeArchitecture;
            var env = new Dictionary<string, string>()
            {
                { "RID", PlatformServices.Default.Runtime.GetRuntimeIdentifier() },
                { "OSNAME", PlatformServices.Default.Runtime.OperatingSystem },
                { "TFM", "dnxcore50" },
                { "OutputDir", Dirs.Output },
                { "Stage1Dir", Dirs.Stage1 },
                { "Stage1CompilationDir", Dirs.Stage1Compilation },
                { "Stage2Dir", Dirs.Stage2 },
                { "STAGE2_DIR", Dirs.Stage2 },
                { "Stage2CompilationDir", Dirs.Stage2Compilation },
                { "HostDir", Dirs.Corehost },
                { "PackageDir", Path.Combine(Dirs.Packages, "dnvm") }, // Legacy name
                { "TestBinRoot", Dirs.TestOutput },
                { "TestPackageDir", Dirs.TestPackages },
                { "MajorVersion", buildVersion.Major.ToString() },
                { "MinorVersion", buildVersion.Minor.ToString() },
                { "PatchVersion", buildVersion.Patch.ToString() },
                { "CommitCountVersion", buildVersion.CommitCountString },
                { "COMMIT_COUNT_VERSION", buildVersion.CommitCountString },
                { "DOTNET_CLI_VERSION", buildVersion.SimpleVersion },
                { "DOTNET_MSI_VERSION", buildVersion.GenerateMsiVersion() },
                { "VersionSuffix", buildVersion.VersionSuffix },
                { "CONFIGURATION", configuration },
                { "ARCHITECTURE", architecture }
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                env["OSNAME"] = "osx";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Cmd("powershell", "-NoProfile", "-NoLogo", Path.Combine(c.BuildContext.BuildDirectory, "scripts", "package", "package.ps1"))
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            }
            else
            {
                // Can directly execute scripts on Unix :). Thank you shebangs!
                Cmd(Path.Combine(c.BuildContext.BuildDirectory, "scripts", "package", "package.sh"))
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            }*/
            return c.Success();
        }

        [Target(nameof(PublishTargets.PublishVersionBadge),
        nameof(PublishTargets.PublishCompressedFile),
        nameof(PublishTargets.PublishInstallerFile))]
        public static BuildTargetResult PublishArtifacts(BuildTargetContext c)
        {
            //version badge



            //zip file

            //Bundle
            return c.Success();
        }

        [Target]
        public static BuildTargetResult PublishVersionBadge(BuildTargetContext c)
        {
            var versionBadge = c.BuildContext.Get<string>("VersionBadge");
            return PublishFile(c, versionBadge);
        }

        [Target]
        public static BuildTargetResult PublishCompressedFile(BuildTargetContext c)
        {
            var compressedFile = c.BuildContext.Get<string>("CompressedFile");
            return PublishFile(c, compressedFile);
        }

        [Target]
        [BuildPlatforms(BuildPlatform.Windows, BuildPlatform.OSX, BuildPlatform.Ubuntu)]
        public static BuildTargetResult PublishInstallerFile(BuildTargetContext c)
        {
            var installerFile = c.BuildContext.Get<string>("InstallerFile");
            return PublishFile(c, installerFile);
        }

        private static BuildTargetResult PublishFile(BuildTargetContext c, string file)
        {
            var env = PackageTargets.GetCommonEnvVars(c);
            Cmd("powershell", "-NoProfile", "-NoLogo",
                Path.Combine(Dirs.RepoRoot, "scripts", "publish", "publish.ps1"), file)
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            return c.Success();
        }
    }
}
