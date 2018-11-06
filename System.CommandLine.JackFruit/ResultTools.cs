namespace System.CommandLine.JackFruit
{
    public class ResultTools
    {
        public static TCli GetResult<TCli, THelper>(string[] args)
        {
            var builder = BuilderTools.Create<TCli, THelper>()
                          .AddStandardDirectives();
            // Create approach to add extra directives

            Parser parser = builder.Build();
            ParseResult result = parser.Parse(args);
            TCli strongResult = GetData<TCli>(result);

            return strongResult;

        }

        private static TCli GetData<TCli>(ParseResult result) => throw new NotImplementedException();
    }
}
