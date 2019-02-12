// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;

namespace System.CommandLine.Binding
{
    public sealed class BindingContext
    {
        private IConsole _console;

        public BindingContext(
            ParseResult parseResult,
            IConsole console = null)
        {
            _console = console ?? new SystemConsole();

            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            ServiceProvider = new ServiceProvider(this);
        }

        public ParseResult ParseResult { get; set; }

        internal IConsoleFactory ConsoleFactory { get; set; }

        internal IHelpBuilder HelpBuilder => (IHelpBuilder)ServiceProvider.GetService(typeof(IHelpBuilder));

        public IConsole Console
        {
            get
            {
                if (ConsoleFactory != null)
                {
                    var consoleFactory = ConsoleFactory;
                    ConsoleFactory = null;
                    _console = consoleFactory.CreateConsole(this);
                }

                return _console;
            }
        }

        internal ServiceProvider ServiceProvider { get; }

        internal bool TryBind(
            IValueDescriptor valueDescriptor,
            IValueSource valueSource,
            out BoundValue boundValue)
        {
            if (valueSource.TryGetValue(valueDescriptor, this, out var value))
            {
                boundValue = new BoundValue(value, valueDescriptor, valueSource);
                return true;
            }
            else
            {
                boundValue = null;
                return false;
            }
        }
    }
}
