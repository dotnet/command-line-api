namespace System.CommandLine.JackFruit
{
    public class PreBinder
    {
        public static Command RootCommand<TRootType>(Func<object, string> descriptionFinder)
        {
            ((DescriptionProvider)PreBinderContext.Current.HelpFinder).AddDescriptionFinder(descriptionFinder);
            var command = CommandProvider.GetCommand(null, typeof(TRootType));
            return command;
        }
    }
}
