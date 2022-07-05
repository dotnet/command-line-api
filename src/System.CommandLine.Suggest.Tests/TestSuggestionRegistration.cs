// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest.Tests
{
    internal class TestSuggestionRegistration : ISuggestionRegistration
    {
        private readonly List<Registration> _suggestionRegistrations = new();

        public TestSuggestionRegistration(params Registration[] suggestionRegistrations)
        {
            foreach (Registration suggestionRegistration in suggestionRegistrations)
            {
                AddSuggestionRegistration(suggestionRegistration);
            }
        }

        public Registration FindRegistration(FileInfo soughtExecutable)
            => _suggestionRegistrations.FirstOrDefault(x => x.ExecutablePath.StartsWith(soughtExecutable.FullName, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Registration> FindAllRegistrations()
            => _suggestionRegistrations;

        public void AddSuggestionRegistration(Registration registration)
        {
            _suggestionRegistrations.Add(registration);
        }
    }
}
