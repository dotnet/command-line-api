// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    internal class TestSuggestionProvider : ISuggestionProvider
    {
        private readonly IReadOnlyCollection<SuggestionRegistration> _findAllRegistrations;
        private readonly SuggestionRegistration _findRegistration;

        public TestSuggestionProvider() : this(
            new SuggestionRegistration("C:\\Program Files\\dotnet\\dotnet.exe", "dotnet complete"))
        { }

        public TestSuggestionProvider(SuggestionRegistration suggestionRegistration)
        {
            _findRegistration = suggestionRegistration;
        }

        public TestSuggestionProvider(IReadOnlyCollection<SuggestionRegistration> findAllRegistrations, SuggestionRegistration findRegistration)
        {
            _findAllRegistrations = findAllRegistrations;
            _findRegistration = findRegistration;
        }

        public SuggestionRegistration FindRegistration(FileInfo soughtExecutable) => _findRegistration;
        public IReadOnlyCollection<SuggestionRegistration> FindAllRegistrations() => _findAllRegistrations ?? new SuggestionRegistration[] { _findRegistration };

        public List<SuggestionRegistration> AddedRegistrations { get; } = new List<SuggestionRegistration>();
        public void AddSuggestionRegistration(SuggestionRegistration registration)
        {
            AddedRegistrations.Add(registration);
        }
    }
}
