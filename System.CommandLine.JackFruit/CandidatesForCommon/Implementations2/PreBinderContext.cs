using System.Collections.Generic;
using System.CommandLine.Invocation;

namespace System.CommandLine.JackFruit
{
    public class PreBinderContext
    {
        private static PreBinderContext current;
        public static PreBinderContext Current
        {
            get
            {
                if (current == null)
                {
                    current = new PreBinderContext();
                }
                return current;
            }
        }
        private IFinder<IEnumerable<Command>> subCommandFinder;
        public IFinder<IEnumerable<Command>> SubCommandFinder
        {
            get => subCommandFinder ?? (subCommandFinder = JackFruit.CommandFinder.Default());
            set => subCommandFinder = value;
        }

        private IFinder<IEnumerable<string>> aliasFinder;
        public IFinder<IEnumerable<string>> AliasFinder
        {
            get => aliasFinder ?? (aliasFinder = JackFruit.AliasFinder.Default());
            set => aliasFinder = value;
        }

        private IFinder<string> helpFinder;
        public IFinder<string> HelpFinder
        {
            get => helpFinder ?? (helpFinder = JackFruit.HelpFinder.Default());
            set => helpFinder = value;
        }

        private IFinder<IEnumerable<Argument>> argumentFinder;
        public IFinder<IEnumerable<Argument>> ArgumentFinder
        {
            get => argumentFinder ?? (argumentFinder = JackFruit.ArgumentFinder.Default());
            set => argumentFinder = value;
        }

        private IFinder<IEnumerable<Option>> optionFinder;
        public IFinder<IEnumerable<Option>> OptionFinder
        {
            get => optionFinder ?? (optionFinder = JackFruit.OptionFinder.Default());
            set => optionFinder = value;
        }

        private IFinder<ICommandHandler> handlerFinder;
        public IFinder<ICommandHandler> HandlerFinder
        {
            get => handlerFinder ?? (handlerFinder = JackFruit.HandlerFinder.Default());
            set => handlerFinder = value;
        }

    }
}
