// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public class GlobalToolsSuggestionRegistration : ISuggestionRegistration
    {
        private readonly string _dotnetProfileDir;
        private readonly IFileEnumerator _fileEnumerator;
        private readonly string _toolsShimPath;

        public GlobalToolsSuggestionRegistration(string dotnetProfileDirectory, IFileEnumerator fileEnumerator = null)
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

        public IReadOnlyCollection<RegistrationPair> FindAllRegistrations()
        {
            return _fileEnumerator.EnumerateFiles(_toolsShimPath).Select(p =>
                new RegistrationPair(p, $"{Path.GetFileNameWithoutExtension(p)} [suggest]")).ToArray();
        }

        public RegistrationPair? FindRegistration(FileInfo soughtExecutable)
        {
            if (soughtExecutable == null) throw new ArgumentNullException(nameof(soughtExecutable));

            if (!soughtExecutable.FullName.StartsWith(_toolsShimPath))
            {
                return null;
            }

            return new RegistrationPair(soughtExecutable.FullName,
                $"{Path.GetFileNameWithoutExtension(soughtExecutable.FullName)} [suggest]");
        }
    }
}
