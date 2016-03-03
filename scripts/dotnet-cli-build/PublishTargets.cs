﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Build.Framework;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

using static Microsoft.DotNet.Cli.Build.Framework.BuildHelpers;

namespace Microsoft.DotNet.Cli.Build
{
    public static class PublishTargets
    {
        private static CloudBlobContainer BlobContainer { get; set; }

        private static string Channel { get; } = "Test";// Environment.GetEnvironmentVariable("RELEASE_SUFFIX");

        private static string Version { get; set; }


        [Target]
        public static BuildTargetResult InitPublish(BuildTargetContext c)
        {            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("CONNECTION_STRING").Trim('"'));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            BlobContainer = blobClient.GetContainerReference("dotnet");
            
            Version = c.BuildContext.Get<BuildVersion>("BuildVersion").SimpleVersion;
            return c.Success();
        }

        [Target(nameof(PrepareTargets.Init),
        nameof(PublishTargets.InitPublish),
        nameof(PublishTargets.PublishArtifacts))]
        [Environment("PUBLISH_TO_AZURE_BLOB", "true")] // This is set by CI systems
        public static BuildTargetResult Publish(BuildTargetContext c)
        {            
            return c.Success();
        }

        [Target(nameof(PublishTargets.PublishVersionBadge),
        nameof(PublishTargets.PublishCompressedFile),
        nameof(PublishTargets.PublishInstallerFile),
        nameof(PublishTargets.PublishLatestVersionTextFile))]
        public static BuildTargetResult PublishArtifacts(BuildTargetContext c)
        {
            return c.Success();
        }

        [Target]
        public static BuildTargetResult PublishVersionBadge(BuildTargetContext c)
        {
            var versionBadge = c.BuildContext.Get<string>("VersionBadge");
            var latestVersionBadgeBlob = $"{Channel}/Binaries/Latest/{Path.GetFileName(versionBadge)}";
            var versionBadgeBlob = $"{Channel}/Binaries/{Version}/{Path.GetFileName(versionBadge)}";

            PublishFileAzure(versionBadgeBlob, versionBadge);
            PublishFileAzure(latestVersionBadgeBlob, versionBadge);
            return c.Success();
        }

        [Target]
        public static BuildTargetResult PublishCompressedFile(BuildTargetContext c)
        {
            var compressedFile = c.BuildContext.Get<string>("CompressedFile");
            var compressedFileBlob = $"{Channel}/Binaries/{Version}/{Path.GetFileName(compressedFile)}";
            var latestCompressedFile = compressedFile.Replace(Version, "latest");
            var latestCompressedFileBlob = $"{Channel}/Binaries/Latest/{Path.GetFileName(latestCompressedFile)}";

            PublishFileAzure(compressedFileBlob, compressedFile);
            PublishFileAzure(latestCompressedFileBlob, compressedFile);
            return c.Success();
        }

        [Target]
        [BuildPlatforms(BuildPlatform.Windows, BuildPlatform.OSX, BuildPlatform.Ubuntu)]
        public static BuildTargetResult PublishInstallerFile(BuildTargetContext c)
        {
            var installerFile = c.BuildContext.Get<string>("InstallerFile");
            var installerFileBlob = $"{Channel}/Installers/{Version}/{Path.GetFileName(installerFile)}";
            var latestInstallerFile = installerFile.Replace(Version, "latest");
            var latestInstallerFileBlob = $"{Channel}/Installers/Latest/{Path.GetFileName(latestInstallerFile)}";

            PublishFileAzure(installerFileBlob, installerFile);
            PublishFileAzure(latestInstallerFileBlob, installerFile);
            return c.Success();
        }

        [Target]        
        public static BuildTargetResult PublishLatestVersionTextFile(BuildTargetContext c)
        {
            var osname = Monikers.GetOSShortName();
            var latestVersionBlob = $"{Channel}/dnvm/latest.{osname}.{CurrentArchitecture.Current}.version";
            var latestVersionFile = Path.Combine(Dirs.Stage2, ".version");

            PublishFileAzure(latestVersionBlob, latestVersionFile);            
            return c.Success();
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

        private static void PublishFileAzure(string blob, string file)
        {   
            CloudBlockBlob blockBlob = BlobContainer.GetBlockBlobReference(blob);
            using (var fileStream = File.OpenRead(file))
            {
                blockBlob.UploadFromStreamAsync(fileStream).Wait();
            }
        }
    }
}
