// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Suggest
{
    public class GlobalToolsSuggestionRegistration : ISuggestionRegistration
    {
        private string _homeDir;

        public GlobalToolsSuggestionRegistration(string homeDirectory)
        {
            _homeDir = homeDirectory;
        }

        public void AddSuggestionRegistration(RegistrationPair registration)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<RegistrationPair> FindAllRegistrations()
        {
            throw new NotImplementedException();
        }

        public RegistrationPair FindRegistration(FileInfo soughtExecutable)
        {
            if (soughtExecutable == null) throw new ArgumentNullException(nameof(soughtExecutable));

            // TODO: Won't work on Windows...Or everyone elses machine.
            // We don't know if the caller is passing the global tools conventional location.
            if (!soughtExecutable.FullName.StartsWith(Path.Combine(_homeDir, ".dotnet/tools")))
            {
                return null;
            }

            return new RegistrationPair(soughtExecutable.FullName, "[suggest]");
        }
    }
}