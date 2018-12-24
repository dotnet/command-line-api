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
        private IListFinder<Command> subCommandFinder;
        public IListFinder<Command> SubCommandFinder
        {
            get => subCommandFinder ?? (subCommandFinder = JackFruit.CommandFinder.Default());
            set => subCommandFinder = value;
        }

        private IListFinder<string> aliasFinder;
        public IListFinder<string> AliasFinder
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

        private IListFinder<Argument> argumentFinder;
        public IListFinder<Argument> ArgumentFinder
        {
            get => argumentFinder ?? (argumentFinder = JackFruit.ArgumentFinder.Default());
            set => argumentFinder = value;
        }

        private IListFinder<Option> optionFinder;
        public IListFinder<Option> OptionFinder
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
