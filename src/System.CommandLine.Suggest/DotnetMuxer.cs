// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.IO;
using System.Runtime.InteropServices;

namespace System.CommandLine.Suggest;

internal static class DotnetMuxer
{
    public static FileInfo Path { get; }

    static DotnetMuxer()
    {
        var muxerFileName = ExecutableName("dotnet");
        var fxDepsFile = GetDataFromAppDomain("FX_DEPS_FILE");

        if (string.IsNullOrEmpty(fxDepsFile))
        {
            return;
        }

        var muxerDir = new FileInfo(fxDepsFile).Directory?.Parent?.Parent?.Parent;

        if (muxerDir is null)
        {
            return;
        }

        var muxerCandidate = new FileInfo(System.IO.Path.Combine(muxerDir.FullName, muxerFileName));

        if (muxerCandidate.Exists)
        {
            Path = muxerCandidate;
        }
        else
        {
            throw new InvalidOperationException("no muxer!");
        }
    }

    public static string GetDataFromAppDomain(string propertyName)
    {
        return AppContext.GetData(propertyName) as string;
    }

    public static string ExecutableName(this string withoutExtension) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? withoutExtension + ".exe"
            : withoutExtension;
}

#endif