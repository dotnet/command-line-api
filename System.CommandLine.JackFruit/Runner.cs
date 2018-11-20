//using System.CommandLine.Builder;
//using System.CommandLine.Invocation;
//using System.Threading.Tasks;

//namespace System.CommandLine.JackFruit
//{
//    public static class Runner
//    {
//        //public static async Task<int> RunAsync<TResult>(string commandLine,
//        //       IDescriptionProvider<Type> helpProvider = null,
//        //       IInvocationProvider invocationProvider = null,
//        //       IRuleProvider ruleProvider = null,
//        //       AliasStyle aliasStyle = AliasStyle.Attribute,
//        //       ArgumentStyle argumentStyle = ArgumentStyle.Attribute)
//        //{
//        //    // TODO: Fix redundancy
//        //    var builder = BuilderTools.CreateBuilder<TResult>(helpProvider, invocationProvider, ruleProvider,
//        //                aliasStyle, argumentStyle)
//        //        .AddStandardDirectives()
//        //        .UseExceptionHandler();
//        //    // Create approach to add extra stuff
//        //    Parser parser = builder.Build();
//        //    return await parser.InvokeAsync(commandLine);

//        //}

//        public static async Task<int> RunAsync<TResult>(string[] args,
//              IDescriptionProvider<Type> descriptionProvider = null,
//              IInvocationProvider invocationProvider = null,
//              IRuleProvider ruleProvider = null,
//              AliasStyle aliasStyle = AliasStyle.Attribute,
//              ArgumentStyle argumentStyle = ArgumentStyle.Attribute)
//        {
//            //var builder = BuilderTools.CreateBuilder<TResult>(helpProvider, invocationProvider, ruleProvider, 
//            //            aliasStyle, argumentStyle)
//            //    .AddStandardDirectives()
//            //    .UseExceptionHandler();

//            var commandProvider = new TypeCommandProvider(descriptionProvider, invocationProvider: invocationProvider);
//            var command = commandProvider.GetRootCommand(typeof(TResult));
//            var builder =new CommandLineBuilder(command)
//                .AddStandardDirectives()
//                .UseExceptionHandler();

//            Parser parser = builder.Build();
//            return await parser.InvokeAsync(args);

//        }

//    }
//}
