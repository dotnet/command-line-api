// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine.Directives;

public abstract class DirectiveSubsystem : CliSubsystem
{
    public string? Value { get; private set; }
    public bool Found { get; private set; }
    public string Id { get; }
    public Location? Location { get; private set; }

    public DirectiveSubsystem(string name, SubsystemKind kind,  IAnnotationProvider? annotationProvider = null, string? id = null)
        : base(name, kind, annotationProvider: annotationProvider)
    {
        Id = id ?? name;
    }

    protected internal override void Initialize(InitializationContext context)
    {
        for (int i = 0; i < context.Args.Count; i++)
        {
            var arg = context.Args[i];
            if (arg[0] == '[') // It looks like a directive, see if it is the one we want
            {
                var start = arg.IndexOf($"[{Id}");
                // Protect against matching substrings, such as "diagramX" matching "diagram" - but longer string may be valid for a different directive and we may still find the one we want
                if (start >= 0)
                {
                    var end = arg.IndexOf("]", start) + 1;
                    var nextChar = arg[start + Id.Length + 1];
                    if (nextChar is ']' or ':')
                    {
                        Found = true;
                        if (nextChar == ':')
                        {
                            Value = arg[(start + Id.Length + 2)..(end - 1)];
                        }
                        Location = new Location(arg.Substring(start, end - start), Location.User, i, null, start);
                        context.Configuration.AddPreprocessedLocation(Location);
                        break;
                    }
                }
            }
            else if (i > 0) // First position might be ExeName, but directives are not legal after other tokens appear
            {
                break;
            }
        }
    }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => Found;

}
