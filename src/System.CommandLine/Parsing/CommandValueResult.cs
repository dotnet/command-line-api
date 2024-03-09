// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Parsing;

public class CommandValueResult
{
    IEnumerable<ValueResult> ValueResults { get; } = new List<ValueResult>();


}
