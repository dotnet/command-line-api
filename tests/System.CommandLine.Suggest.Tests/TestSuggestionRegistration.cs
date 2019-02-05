// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest.Tests
{
    internal class TestSuggestionRegistration : ISuggestionRegistration
    {
        private readonly List<RegistrationPair> _suggestionRegistrations = new List<RegistrationPair>();

        public TestSuggestionRegistration(params RegistrationPair[] suggestionRegistrations)
        {
            foreach (RegistrationPair suggestionRegistration in suggestionRegistrations)
            {
                AddSuggestionRegistration(suggestionRegistration);
            }
        }

        public RegistrationPair FindRegistration(FileInfo soughtExecutable)
            => _suggestionRegistrations.FirstOrDefault(x => x.CommandPath.StartsWith(soughtExecutable.FullName, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<RegistrationPair> FindAllRegistrations()
            => _suggestionRegistrations;

        public void AddSuggestionRegistration(RegistrationPair registration)
        {
            _suggestionRegistrations.Add(registration);
        }
    }
}
