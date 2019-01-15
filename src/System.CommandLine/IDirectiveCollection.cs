// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public interface IDirectiveCollection : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        bool Contains(string name);

        bool TryGetValues(string name, out IEnumerable<string> values);
    }
}
