// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Suggest
{
    public class GlobalToolsSuggestionRegistration : ISuggestionRegistration
    {
        private readonly string _dotnetProfileDir;

        public GlobalToolsSuggestionRegistration(string dotnetProfileDirectory)
        {
            _dotnetProfileDir = dotnetProfileDirectory;
        }

        public void AddSuggestionRegistration(RegistrationPair registration)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<RegistrationPair> FindAllRegistrations()
        {
            throw new NotImplementedException();
        }

        public RegistrationPair? FindRegistration(FileInfo soughtExecutable)
        {
            if (soughtExecutable == null) throw new ArgumentNullException(nameof(soughtExecutable));

            if (!soughtExecutable.FullName.StartsWith(Path.Combine(_dotnetProfileDir, "tools")))
            {
                return null;
            }

            return new RegistrationPair(soughtExecutable.FullName,
                $"{Path.GetFileNameWithoutExtension(soughtExecutable.FullName)} [suggest]");
        }
    }
}
