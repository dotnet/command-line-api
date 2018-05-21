// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.CompletionSuggestions
{
    public class SuggestionFileProvider : ISuggestionFileProvider
    {
        readonly List<string> _registrationConfigFilePaths = new List<string>
             {
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "System.CommandLine.Completion.txt")
            };

        public IReadOnlyCollection<string> RegistrationConfigurationFilePaths => _registrationConfigFilePaths;

        public void AddRegistrationConfigurationFilePath(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentNullException(nameof(configFilePath));
            }
            _registrationConfigFilePaths.Add(configFilePath);
        }

        public string FindRegistration(FileInfo soughtExecutable)
        {
            foreach (string configFilePath in RegistrationConfigurationFilePaths)
            {
                if (!File.Exists(configFilePath))
                {
                    continue;
                }

                string completionTarget = File.ReadAllLines(configFilePath).SingleOrDefault(line =>
                    line.StartsWith(soughtExecutable.FullName, StringComparison.OrdinalIgnoreCase));

                if (completionTarget != null)
                {
                    return completionTarget;
                }
            }

            // Completion provider not found!
            return null;
        }
    }
}
