// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.DragonFruit
{
    public class CommandHelpMetadata
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> ParameterDescriptions { get; } = new Dictionary<string, string>();
    }
}
