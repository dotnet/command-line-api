using System.CommandLine.Subsystems.Annotations;
using System.Text.RegularExpressions;
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.DataTraits;

public class RegexData : DataTrait
{
    public RegexData(string regexText)
    {
        RegexText = regexText;
    }

    public RegexData(Text.RegularExpressions.Regex? regex)
    {
        Regex = regex;
    }

    public Text.RegularExpressions.Regex? Regex {  get; }

    public string? RegexText { get; }
}
