// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace System.CommandLine.Parsing
{
    public record Location
    {
        public const string Implicit = "Implicit";
        public const string Internal = "Internal";
        public const string User = "User";
        public const string Response = "Response";

        internal static Location CreateRoot(string exeName, bool isImplicit, int start)
            => new(exeName, isImplicit ? Internal : User, start, null);
        internal static Location CreateImplicit(string text, Location outerLocation, int offset = 0)
           => new(text, Implicit, -1, outerLocation, offset);
        internal static Location CreateInternal(string text, Location? outerLocation = null, int offset = 0)
           => new(text, Internal, -1, outerLocation, offset);
        internal static Location CreateUser(string text, int start, Location outerLocation, int offset = 0)
            => new(text, User, start, outerLocation, offset);
        internal static Location CreateResponse(string responseSourceName, int start, Location outerLocation, int offset = 0)
            => new(responseSourceName, $"{Response}:{responseSourceName}", start, outerLocation, offset);

        internal static Location FromOuterLocation(string text, int start, Location outerLocation, int offset = 0)
            => new(text, outerLocation.Source, start, outerLocation, offset);

        public Location(string text, string source, int start, Location? outerLocation, int offset = 0)
        {
            Text = text;
            Source = source;
            Start = start;
            Length = text.Length;
            Offset = offset;
            OuterLocation = outerLocation;
        }

        public string Text { get; }
        public string Source { get; }
        public int Start { get; }
        public int Offset { get; }
        public int Length { get; }
        public Location? OuterLocation { get; }

        public bool IsImplicit
            => Source == Implicit;

        public override string ToString()
            => $"{(OuterLocation is null ? "" : OuterLocation.ToString() + "; ")}{Source} [{Start}, {Length}, {Offset}]";

    }
}