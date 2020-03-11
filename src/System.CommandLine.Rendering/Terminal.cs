namespace System.CommandLine.Rendering
{
    public static class Terminal
    {
        public static void Render(
            this ITerminal terminal,
            TextSpan span,
            Region region = null)
        {
            if (terminal is IRenderable t)
            {
                var renderer = new ConsoleRenderer(terminal, t.OutputMode);

                renderer.RenderToRegion(span, region ?? t.GetRegion());
            }
        }

        public static void Render(
            this ITerminal terminal,
            FormattableString value,
            Region region = null)
        {
            if (terminal is IRenderable t)
            {
                var renderer = new ConsoleRenderer(terminal, t.OutputMode);

                var span = renderer.Formatter.Format(value);

                renderer.RenderToRegion(span, region ?? t.GetRegion());
            }
        }

        public static ITerminal GetTerminal(
            this IConsole console,
            bool preferVirtualTerminal = true,
            OutputMode outputMode = OutputMode.Auto)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console is ITerminal t)
            {
                return t;
            }

            if (console.IsOutputRedirected)
            {
                return null;
            }

            ITerminal terminal;

            if (preferVirtualTerminal &&
                VirtualTerminalMode.TryEnable() is VirtualTerminalMode virtualTerminalMode &&
                virtualTerminalMode.IsEnabled)
            {
                terminal = new VirtualTerminal(
                    console,
                    virtualTerminalMode);
            }
            else
            {
                terminal = new SystemConsoleTerminal(console);
            }

            if (terminal is TerminalBase terminalBase)
            {
                terminalBase.OutputMode = outputMode;
            }

            return terminal;
        }
    }
}
