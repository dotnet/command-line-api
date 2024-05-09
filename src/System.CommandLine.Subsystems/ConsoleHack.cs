// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace System.CommandLine;

public class ConsoleHack
{
    private readonly StringBuilder buffer = new();
    private bool redirecting = false;

    public void WriteLine(string text = "")
    {
        if (redirecting)
        {
            buffer.AppendLine(text);
        }
        else
        {
            Console.WriteLine(text);
        }
    }

    public string GetBuffer() => buffer.ToString();

    public void ClearBuffer() => buffer.Clear();

    public ConsoleHack RedirectToBuffer(bool shouldRedirect)
    {
        redirecting = shouldRedirect;
        return this;
    }
}
