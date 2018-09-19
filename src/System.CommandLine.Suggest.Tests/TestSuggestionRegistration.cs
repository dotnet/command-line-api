// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest.Tests
{
    internal class TestSuggestionRegistration : ISuggestionRegistration
    {
        private readonly List<SuggestionRegistration> _suggestionRegistrations;

        public TestSuggestionRegistration(params SuggestionRegistration[] suggestionRegistrations)
        {
            _suggestionRegistrations = new List<SuggestionRegistration>();

            foreach (SuggestionRegistration suggestionRegistration in suggestionRegistrations)
            {
                AddSuggestionRegistration(suggestionRegistration);
            }
        }

        public SuggestionRegistration FindRegistration(FileInfo soughtExecutable)
            => _suggestionRegistrations.FirstOrDefault(x => x.CommandPath.StartsWith(soughtExecutable.FullName, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyCollection<SuggestionRegistration> FindAllRegistrations()
            => _suggestionRegistrations.AsReadOnly();

        public void AddSuggestionRegistration(SuggestionRegistration registration)
        {
            _suggestionRegistrations.Add(registration);
        }
    }
}
