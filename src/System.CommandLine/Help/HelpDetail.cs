// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class HelpDetail : IHelpDetail
    {
        internal const bool DefaultIsHidden = false;

        public string Description { get; set; }

        public bool IsHidden { get; set; }

        public string Name { get; set; }
    }
}
