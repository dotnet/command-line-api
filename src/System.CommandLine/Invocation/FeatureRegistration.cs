// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class FeatureRegistration
    {
        private static readonly string _assemblyName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).FullName;
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
            if (!_sentinelFile.Directory.Exists)
            {
                _sentinelFile.Directory.Create();
            }

            if (!_sentinelFile.Exists)
            {
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
