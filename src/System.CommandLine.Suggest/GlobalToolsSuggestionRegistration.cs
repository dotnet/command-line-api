// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Suggest
{
    public class GlobalToolsSuggestionRegistration : ISuggestionRegistration
    {
        private readonly string _nullableToolsShimPath;
        private readonly IEnumerable<string> _filesNameWithoutExtensionUnderDotnetProfileTools;

        public GlobalToolsSuggestionRegistration(string dotnetProfileDirectory = null,
            IEnumerable<string> filesNameWithoutExtensionUnderDotnetProfileTools = null)
        {
            var directory = dotnetProfileDirectory;
            if (directory == null)
            {
                DotnetProfileDirectory.TryGet(out directory);
            }

            _nullableToolsShimPath = directory != null
                ? Path.Combine(directory, "tools")
                : null;

            _filesNameWithoutExtensionUnderDotnetProfileTools 
                = filesNameWithoutExtensionUnderDotnetProfileTools ?? FileEnumerator.EnumerateFilesWithoutExtension(new DirectoryInfo(_nullableToolsShimPath));
        }

        public void AddSuggestionRegistration(Registration registration)
        {
        }

        public IEnumerable<Registration> FindAllRegistrations()
        {
            if (_nullableToolsShimPath == null)
            {
                return Array.Empty<Registration>();
            }

            return _filesNameWithoutExtensionUnderDotnetProfileTools.Select(p =>
                new Registration(Path.Combine(_nullableToolsShimPath, p)));
        }

        public Registration FindRegistration(FileInfo soughtExecutable)
        {
            if (soughtExecutable == null)
            {
                throw new ArgumentNullException(nameof(soughtExecutable));
            }

            if (_nullableToolsShimPath == null)
            {
                return null;
            }

            if (!soughtExecutable.FullName.StartsWith(_nullableToolsShimPath))
            {
                return null;
            }

            return new Registration(soughtExecutable.FullName);
        }
    }
}
