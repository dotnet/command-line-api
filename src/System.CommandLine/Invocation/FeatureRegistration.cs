// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class FeatureRegistration
    {
        private static readonly string? _assemblyName = RootCommand.GetAssembly().FullName;

        private readonly FileInfo _sentinelFile;

        public FeatureRegistration(string featureName)
        {
            _sentinelFile = new FileInfo(
                Path.Combine(
                    Path.GetTempPath(),
                    "system-commandline-sentinel-files",
                    $"{featureName}-{_assemblyName}"));
        }

        public async Task EnsureRegistered(Func<Task<string>> onInitialize)
        {
            if (!_sentinelFile.Exists)
            {
                if (_sentinelFile.Directory is { Exists: false })
                {
                    _sentinelFile.Directory.Create();
                }

                try
                {
                    var message = await onInitialize();

                    File.WriteAllText(
                        _sentinelFile.FullName,
                        message);
                }
                catch (Exception)
                {
                    // fail silently
                }
            }
        }
    }
}
