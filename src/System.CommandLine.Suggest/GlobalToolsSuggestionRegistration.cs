// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public class GlobalToolsSuggestionRegistration : ISuggestionRegistration
    {
        private readonly IFileEnumerator _fileEnumerator;
        private readonly string _nullableToolsShimPath;

        public GlobalToolsSuggestionRegistration(string dotnetProfileDirectory = null,
            IFileEnumerator fileEnumerator = null)
        {
            var directory = dotnetProfileDirectory;
            if (directory == null)
            {
                DotnetProfileDirectory.TryGet(out directory);
            }

            _nullableToolsShimPath = directory != null
                    ? Path.Combine(directory, "tools")
                    : null;

            _fileEnumerator = fileEnumerator ?? new FileEnumerator();
        }

        public void AddSuggestionRegistration(RegistrationPair registration)
        {
        }

        public IEnumerable<RegistrationPair> FindAllRegistrations()
        {
            if (_nullableToolsShimPath == null)
            {
                return Array.Empty<RegistrationPair>();
            }

            return _fileEnumerator.EnumerateFiles(_nullableToolsShimPath).Select(p =>
                new RegistrationPair(p, $"{Path.GetFileNameWithoutExtension(p)} [suggest]"));
        }

        public RegistrationPair? FindRegistration(FileInfo soughtExecutable)
        {
            if (_nullableToolsShimPath == null)
            {
                return null;
            }

            if (soughtExecutable == null) throw new ArgumentNullException(nameof(soughtExecutable));

            if (!soughtExecutable.FullName.StartsWith(_nullableToolsShimPath))
            {
                return null;
            }

            return new RegistrationPair(soughtExecutable.FullName,
                $"{Path.GetFileNameWithoutExtension(soughtExecutable.FullName)} [suggest]");
        }
    }
}
