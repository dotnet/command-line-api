namespace System.CommandLine.JackFruit
{
    public class PreBinder
    {
        public static Command RootCommand<TRootType>(Func<Command[], object,(bool, string)> descriptionFinder)
        {
            PreBinderContext.Current.DescriptionProvider.AddStrategy(descriptionFinder);
            var command = CommandStrategies.GetCommand(null, typeof(TRootType));
            return command;
        }
    }
}
