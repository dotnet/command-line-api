// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public class CombineSuggestionRegistration : ISuggestionRegistration
    {
        private readonly ISuggestionRegistration[] _suggestionRegistrations;

        public CombineSuggestionRegistration(params ISuggestionRegistration[] suggestionRegistration)
        {
            _suggestionRegistrations =
                suggestionRegistration ?? throw new ArgumentNullException(nameof(suggestionRegistration));
        }

        public void AddSuggestionRegistration(RegistrationPair registration)
        {
            foreach (var suggestionRegistration in _suggestionRegistrations)
            {
                suggestionRegistration.AddSuggestionRegistration(registration);
            }
        }

        public RegistrationPair? FindRegistration(FileInfo soughtExecutable)
        {
            return _suggestionRegistrations
                .Select(s => s.FindRegistration(soughtExecutable))
                .FirstOrDefault(r => r.HasValue);
        }

        public IEnumerable<RegistrationPair> FindAllRegistrations()
        {
            return _suggestionRegistrations
                .SelectMany(s => s.FindAllRegistrations());
        }
    }
}
