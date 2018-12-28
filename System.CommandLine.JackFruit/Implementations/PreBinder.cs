namespace System.CommandLine.JackFruit
{
    public class PreBinder
    {
        public static Command RootCommand<TRootType>(IDescriptionFinder descriptionFinder = null)
        {
            ((HelpFinder)PreBinderContext.Current.HelpFinder).AddDescriptionFinder(descriptionFinder);
            var command = CommandFinder.GetCommand(null, typeof(TRootType));
            return command;
        }
    }
}
