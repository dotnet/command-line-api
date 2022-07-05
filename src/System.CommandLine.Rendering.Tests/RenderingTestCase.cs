// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Tests
{
    public class RenderingTestCase
    {
        private static readonly TextSpanFormatter _formatter = new();

        public RenderingTestCase(
            string name,
            FormattableString rendering,
            Region inRegion,
            params TextRendered[] expectOutput)
        {
            if (rendering == null)
            {
                throw new ArgumentNullException(nameof(rendering));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            InputSpan = _formatter.ParseToSpan(rendering);
            Region = inRegion ?? throw new ArgumentNullException(nameof(inRegion));
            ExpectedOutput = expectOutput ?? throw new ArgumentNullException(nameof(expectOutput));
        }

        public string Name { get; }

        public TextSpan InputSpan { get; }

        public Region Region { get; }

        public TextRendered[] ExpectedOutput { get; }

        public override string ToString() => $"{Name} (in {Region})";
    }
}
