// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public abstract class TerminalBase :
        SystemTerminal,
        ITerminal
    {
        private ConsoleRenderer renderer;

        protected TerminalBase(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public abstract void Clear();

        public abstract int CursorLeft { get; set; }

        public abstract int CursorTop { get; set; }

        public IConsole Console { get; }

        public abstract void SetCursorPosition(int left, int top);

        public OutputMode OutputMode { get; set; } = OutputMode.Auto;

        public virtual void Render(Span span, Region region = null)
        {
            renderer = renderer ?? new ConsoleRenderer(this, OutputMode);

            renderer.RenderToRegion(span, region ?? GetRegion());
        }

        public Region GetRegion() =>
            IsOutputRedirected
                ? new Region(0, 0, int.MaxValue, int.MaxValue, false)
                : EntireConsoleRegion.Instance;
    }
}
