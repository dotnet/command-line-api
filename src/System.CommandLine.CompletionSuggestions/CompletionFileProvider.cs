// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.CompletionSuggestions
{
    public interface ICompletionFileProvider
    {
        IReadOnlyCollection<string> RegistrationConfigFilePaths { get; }
        void AddRegistrationConfigFilePath(string configFilePath);
        string FindCompletionRegistration(FileInfo soughtExecutible);
    }

    public class CompletionFileProvider : ICompletionFileProvider
    {
        readonly List<string> _registrationConfigFilePaths = new List<string>
             {
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "System.CommandLine.Completion.txt")
            };

        public IReadOnlyCollection<string> RegistrationConfigFilePaths => _registrationConfigFilePaths;

        public void AddRegistrationConfigFilePath(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentNullException(nameof(configFilePath));
            }
            _registrationConfigFilePaths.Add(configFilePath);
        }

        public string FindCompletionRegistration(FileInfo soughtExecutible)
        {
            foreach (string configFilePath in RegistrationConfigFilePaths)
            {
                if (!File.Exists(configFilePath))
                {
                    continue;
                }

                string completionTarget = File.ReadAllLines(configFilePath).SingleOrDefault(line =>
                    line.StartsWith(soughtExecutible.FullName, StringComparison.OrdinalIgnoreCase));

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
