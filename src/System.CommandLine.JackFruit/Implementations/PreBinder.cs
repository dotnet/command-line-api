namespace System.CommandLine.JackFruit
{
    public class PreBinder
    {
        public static Command RootCommand<TRootType>(Func<Command, object, string> descriptionFinder)
        {
            PreBinderContext.Current.DescriptionStrategies.AddStrategy(descriptionFinder);
            var command = CommandProvider.GetCommand(null, typeof(TRootType));
            return command;
        }
    }
}
