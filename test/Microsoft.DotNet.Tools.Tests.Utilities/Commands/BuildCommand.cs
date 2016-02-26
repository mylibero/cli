﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.DotNet.Cli.Utils;
using System.Runtime.InteropServices;
using Microsoft.DotNet.ProjectModel;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public sealed class BuildCommand : TestCommand
    {
        private Project _project;
        private string _projectPath;
        private string _outputDirectory;
        private string _buidBasePathDirectory;
        private string _configuration;
        private string _framework;
        private string _versionSuffix;
        private bool _noHost;
        private bool _buildProfile;
        private bool _noIncremental;
        private bool _noDependencies;

        private string OutputOption
        {
            get
            {
                return _outputDirectory == string.Empty ?
                                           "" :
                                           $"-o \"{_outputDirectory}\"";
            }
        }

        private string BuildBasePathOption
        {
            get
            {
                return _buidBasePathDirectory == string.Empty ?
                                           "" :
                                           $"-b {_buidBasePathDirectory}";
            }
        }

        private string ConfigurationOption
        {
            get
            {
                return _configuration == string.Empty ?
                                           "" :
                                           $"-c {_configuration}";
            }
        }
        private string FrameworkOption
        {
            get
            {
                return _framework == string.Empty ?
                                           "" :
                                           $"--framework {_framework}";
            }
        }
        
        private string VersionSuffixOption
        {
            get
            {
                return _versionSuffix == string.Empty ?
                                    "" :
                                    $"--version-suffix {_versionSuffix}";
            }
        }

        private string NoHostOption
        {
            get
            {
                return _noHost ?
                        "--no-host" :
                        "";
            }
        }

        private string BuildProfile
        {
            get
            {
                return _buildProfile ?
                    "--build-profile" :
                    "";
            }
        }

        private string NoIncremental
        {
            get
            {
                return _noIncremental ?
                    "--no-incremental" :
                    "";
            }
        }

        private string NoDependencies
        {
            get
            {
                return _noDependencies ?
                    "--no-dependencies" :
                    "";
            }
        }

        public BuildCommand(
            string projectPath,
            string output="",
            string buidBasePath="",
            string configuration="",
            string framework="",
            string versionSuffix="",
            bool noHost=false,
            bool buildProfile=true,
            bool noIncremental=false,
            bool noDependencies=false
            )
            : base("dotnet")
        {
            _projectPath = projectPath;
            _project = ProjectReader.GetProject(projectPath);

            _outputDirectory = output;
            _buidBasePathDirectory = buidBasePath;
            _configuration = configuration;
            _versionSuffix = versionSuffix;
            _framework = framework;
            _noHost = noHost;
            _buildProfile = buildProfile;
            _noIncremental = noIncremental;
            _noDependencies = noDependencies;
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"--verbose build {BuildArgs()} {args}";
            return base.Execute(args);
        }

        public override CommandResult ExecuteWithCapturedOutput(string args = "")
        {
            args = $"--verbose build {BuildArgs()} {args}";
            return base.ExecuteWithCapturedOutput(args);
        }

        public string GetOutputExecutableName()
        {
            var result = _project.Name;
            result += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
            return result;
        }

        private string BuildArgs()
        {
            return $"{BuildProfile} {NoDependencies} {NoIncremental} \"{_projectPath}\" {OutputOption} {BuildBasePathOption} {ConfigurationOption} {FrameworkOption} {VersionSuffixOption} {NoHostOption}";
        }
    }
}
