using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Build.Framework;
using Microsoft.Extensions.PlatformAbstractions;

using static Microsoft.DotNet.Cli.Build.Framework.BuildHelpers;

namespace Microsoft.DotNet.Cli.Build
{
    public static class PackageTargets
    {
        [Target]
        public static BuildTargetResult InitPackage(BuildTargetContext c)
        {
            if (!Directory.Exists(Dirs.Packages))
            {
                Directory.CreateDirectory(Dirs.Packages);
            }
                        
            return c.Success();
        }

        [Target(nameof(PrepareTargets.Init),
        nameof(PackageTargets.InitPackage),
        nameof(PackageTargets.GenerateVersionBadge),
        nameof(PackageTargets.GenerateCompressedFile),
        nameof(PackageTargets.GenerateInstaller),
        nameof(PackageTargets.GenerateNugetPackages))]
        [Environment("DOTNET_BUILD_SKIP_PACKAGING", null)]
        public static BuildTargetResult Package(BuildTargetContext c)
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
        
        [Target]
        public static BuildTargetResult GenerateVersionBadge(BuildTargetContext c)
        {
            var buildVersion = c.BuildContext.Get<BuildVersion>("BuildVersion");
            var versionSvg = Path.Combine(Dirs.RepoRoot, "resources", "images", "version_badge.svg");
            var outputVersionSvg = c.BuildContext.Get<string>("VersionBadge");

            var versionSvgContent = File.ReadAllText(versionSvg);
            versionSvgContent = versionSvgContent.Replace("ver_number", buildVersion.SimpleVersion);            
            File.WriteAllText(outputVersionSvg, versionSvgContent);

            return c.Success();
        }
        
        [Target(nameof(PackageTargets.GenerateZip), nameof(PackageTargets.GenerateTarBall))]
        public static BuildTargetResult GenerateCompressedFile(BuildTargetContext c)
        {   
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.Windows)]
        public static BuildTargetResult GenerateZip(BuildTargetContext c)
        {
            var zipFile = c.BuildContext.Get<string>("CompressedFile");

            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);
            }

            ZipFile.CreateFromDirectory(Dirs.Stage2, zipFile, CompressionLevel.Optimal, false);
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.Unix)]
        public static BuildTargetResult GenerateTarBall(BuildTargetContext c)
        {
            Directory.CreateDirectory(Dirs.Packages);
            var tarFile = Path.Combine(Dirs.Packages, GetProductMoniker(c) + ".tar.gz");
            var gitResult = Cmd("tar", "-czf", tarFile, "-C", Dirs.Stage2, ".")
                            .Execute();
            gitResult.EnsureSuccessful();            
            return c.Success();
        }
        
        [Target(nameof(PackageTargets.GenerateMsi),
        nameof(PackageTargets.GeneratePkg),
        nameof(PackageTargets.GenerateDeb))]
        public static BuildTargetResult GenerateInstaller(BuildTargetContext c)
        {
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.Windows)]
        public static BuildTargetResult GenerateMsi(BuildTargetContext c)
        {
            var env = GetCommonEnvVars(c);
            Cmd("powershell", "-NoProfile", "-NoLogo",
                Path.Combine(Dirs.RepoRoot, "packaging", "windows", "generatemsi.ps1"), Dirs.Stage2)
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.OSX)]
        public static BuildTargetResult GeneratePkg(BuildTargetContext c)
        {
            var env = GetCommonEnvVars(c);
            Cmd(Path.Combine(Dirs.RepoRoot, "packaging", "osx", "package-osx.sh"))
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult GenerateDeb(BuildTargetContext c)
        {
            var env = GetCommonEnvVars(c);
            Cmd(Path.Combine(Dirs.RepoRoot, "scripts", "package", "package-debian.sh"))
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            return c.Success();
        }

        [Target]
        [BuildPlatforms(BuildPlatform.Windows)]
        public static BuildTargetResult GenerateNugetPackages(BuildTargetContext c)
        {
            var buildVersion = c.BuildContext.Get<BuildVersion>("BuildVersion");
            var versionSuffix = buildVersion.VersionSuffix;
            var env = GetCommonEnvVars(c);
            Cmd("powershell", "-NoProfile", "-NoLogo",
                Path.Combine(Dirs.RepoRoot, "packaging", "nuget", "package.ps1"), Path.Combine(Dirs.Stage2, "bin"), versionSuffix)
                    .Environment(env)
                    .Execute()
                    .EnsureSuccessful();
            return c.Success();
        }

        private static string GetProductMoniker(BuildTargetContext c)
        {
            string osname = "";
            switch(CurrentPlatform.Current)
            {
                case BuildPlatform.Windows:
                    osname = "win";
                    break;
                default:
                    osname = CurrentPlatform.Current.ToString().ToLower();
                    break;
            }            
            var arch = CurrentArchitecture.Current.ToString();
            var version = c.BuildContext.Get<BuildVersion>("BuildVersion").SimpleVersion;
            return $"dotnet-{osname}-{arch}.{version}";
        }
        
        internal static Dictionary<string, string> GetCommonEnvVars(BuildTargetContext c)
        {
            // Set up the environment variables previously defined by common.sh/ps1
            // This is overkill, but I want to cover all the variables used in all OSes (including where some have the same names)
            var buildVersion = c.BuildContext.Get<BuildVersion>("BuildVersion");
            var configuration = c.BuildContext.Get<string>("Configuration");
            var architecture = PlatformServices.Default.Runtime.RuntimeArchitecture;
            var env = new Dictionary<string, string>()
            {
                { "RID", PlatformServices.Default.Runtime.GetRuntimeIdentifier() },
                { "OSNAME", PlatformServices.Default.Runtime.OperatingSystem },
                { "TFM", "dnxcore50" },
                { "REPOROOT", Dirs.RepoRoot },
                { "OutputDir", Dirs.Output },
                { "Stage1Dir", Dirs.Stage1 },
                { "Stage1CompilationDir", Dirs.Stage1Compilation },
                { "Stage2Dir", Dirs.Stage2 },
                { "STAGE2_DIR", Dirs.Stage2 },
                { "Stage2CompilationDir", Dirs.Stage2Compilation },
                { "HostDir", Dirs.Corehost },
                { "PackageDir", Path.Combine(Dirs.Packages) }, // Legacy name
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
            
            return env;
        }
    }
}
