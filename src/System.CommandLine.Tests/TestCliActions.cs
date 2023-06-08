// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Tests;

public class SynchronousTestAction : SynchronousCliAction
{
    private readonly Action<ParseResult> _invoke;

    public SynchronousTestAction(Action<ParseResult> invoke, bool terminating = true)
    {
        _invoke = invoke;
        Terminating = terminating;
    }

    public override int Invoke(ParseResult parseResult)
    {
        _invoke(parseResult);
        return 0;
    }
}

public class AsynchronousTestAction : AsynchronousCliAction
{
    private readonly Action<ParseResult> _invoke;

    public AsynchronousTestAction(Action<ParseResult> invoke, bool terminating = true)
    {
        _invoke = invoke;
        Terminating = terminating;
    }

    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _invoke(parseResult);
        return Task.FromResult(0);
    }
}