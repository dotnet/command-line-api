//using System.Collections.Generic;
//using System.CommandLine.Builder;
//using System.CommandLine.Invocation;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace System.CommandLine.JackFruit
//{
//    public class BuilderTools
//    {

//        public static CommandLineBuilder CreateBuilder<TRoot>(
//              IDescriptionProvider<Type> helpProvider = null,
//              IInvocationProvider invocationProvider = null,
//              IRuleProvider ruleProvider = null,
//              AliasStyle aliasStyle = AliasStyle.Attribute,
//              ArgumentStyle argumentStyle = ArgumentStyle.Attribute)
//        {
//            var builderTools = new BuilderTools(typeof(TRoot), helpProvider, invocationProvider,
//                ruleProvider, aliasStyle, argumentStyle);
//            return builderTools.CreateBuilder();
//        }

//        private readonly Type rootType;
//        private readonly IDescriptionProvider<Type> helpProvider;
//        private readonly IInvocationProvider invocationProvider;
//        private readonly IRuleProvider ruleProvider;
//        private readonly AliasStyle aliasStyle;
//        private readonly ArgumentStyle argumentStyle;
//        private readonly IEnumerable<IGrouping<Type, Type>> typesByBase;

//        private BuilderTools(Type rootType,
//                IDescriptionProvider<Type> helpProvider = null,
//                IInvocationProvider invocationProvider = null,
//                IRuleProvider ruleProvider = null,
//                AliasStyle aliasStyle = AliasStyle.Attribute,
//                ArgumentStyle argumentStyle = ArgumentStyle.Attribute)
//        {
//            this.rootType = rootType;
//            this.helpProvider = helpProvider;
//            this.invocationProvider = invocationProvider;
//            this.ruleProvider = ruleProvider;
//            this.aliasStyle = aliasStyle;
//            this.argumentStyle = argumentStyle;

//            typesByBase = rootType.Assembly
//                      .GetTypes()
//                      .GroupBy(x => x.BaseType);
//        }

//        internal CommandLineBuilder CreateBuilder()
//            => new CommandLineBuilder()
//                    .AddCommands(CreateSubCommands(typesByBase, rootType))
//                    .AddStandardDirectives();

//        private IEnumerable<Command> CreateSubCommands(IEnumerable<IGrouping<Type, Type>> typesByBase, Type currentType)
//        {
//            var derivedTypes = typesByBase
//                               .Where(x => x.Key == currentType)
//                               .SingleOrDefault();
//            var list = new List<Command>();
//            if (derivedTypes == null)
//            {
//                // At the end of recursion
//            }
//            else
//            {
//                foreach (var derivedType in derivedTypes)
//                {
//                    list.Add(CreateCommand(typesByBase, derivedType));
//                }
//            }
//            return list;
//        }

//        private Command CreateCommand(IEnumerable<IGrouping<Type, Type>> typesByBase, Type currentType)
//        {
//            var command = new Command(
//                name: GetName(currentType),
//                description: GetHelp(currentType));
//            SetHandler(command, currentType);

//            var properties = currentType.GetProperties();
//            foreach (var property in properties)
//            {
//                if (Skip(property))
//                {
//                    continue;
//                }
//                if (IsArgument(property))
//                {
//                    command.Argument = GetArgument(currentType, property);
//                }
//                else
//                {
//                    command.AddOption(GetOption(currentType, property));
//                }
//            }
//            if (command.Argument == null)
//            {
//                var ctor = currentType
//                            .GetConstructors()
//                            .Where(c => c.GetParameters().Count() == 1)
//                            .FirstOrDefault();
//                if (ctor != null)
//                {
//                    var param = ctor.GetParameters().First();
//                    command.Argument = GetArgument(currentType, param);
//                }
//            }
//            return command
//                .AddCommands(CreateSubCommands(typesByBase, currentType));
//        }

//        private bool Skip(PropertyInfo property)
//            => property.CustomAttributes
//                .Any(x => x.AttributeType == typeof(IgnoreAttribute));

//        private Option GetOption(Type currentType, PropertyInfo property)
//        {
//            var option = TypeBinder.BuildOption(property);
//            option.Help.Description = GetHelp(currentType, property);
//            var aliasAttribute = property.GetCustomAttribute<AliasAttribute>();
//            if (aliasAttribute != null)
//            {
//                foreach (var alias in aliasAttribute.Aliases)
//                {
//                    option.AddAlias(alias);
//                }
//            }
//            else
//            {
//                var name = option.Name;
//                bool foundAlias = false;
//                // Note not iterating to end so remaining character is guaranteed
//                for (int i = 0; i < name.Length - 1; i++)
//                {

//                    if (name[i] == '_')
//                    {
//                        option.AddAlias(name[i + 1].ToString());
//                        foundAlias = true;
//                    }
//                }
//                if (foundAlias)
//                {
//                    option.Name = option.Name.Replace("_", "");
//                }
//            }
//            return option;
//        }

//        private Argument GetArgument(Type currentType, PropertyInfo property)
//        {
//            var argument = new Argument
//            {
//                ArgumentType = property.PropertyType
//            };
//            argument.Help.Name = property.Name.EndsWith("Arg")
//                        ? property.Name.Substring(0, property.Name.Length - 3)
//                        : property.Name;
//            argument.Help.Description = GetHelp(currentType, property);
//            return argument;
//        }

//        private Argument GetArgument(Type currentType, ParameterInfo parameter)
//        {
//            var argument = new Argument
//            {
//                ArgumentType = parameter.ParameterType
//            };
//            argument.Help.Name = parameter.Name;
//            argument.Help.Description = GetHelp(currentType, parameter);
//            return argument;
//        }

//        private bool IsArgument(PropertyInfo property)
//            => property.Name.EndsWith("Arg") ||
//            property.CustomAttributes
//                .Any(x => x.AttributeType == typeof(ArgumentAttribute));

//        private void SetHandler(Command command, Type currentType)
//        {
//            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
//            var methodInfo = this.GetType().GetMethod(nameof(SetHandlerInternal), bindingFlags);
//            var constructedMethod = methodInfo.MakeGenericMethod(currentType);
//            constructedMethod.Invoke(this, new object[] { command });
//        }

//        private void SetHandlerInternal<TResult>(Command command)
//        {
//            Func<TResult, Task<int>> invocation = null;
//            if (invocationProvider != null)
//            {
//                invocation = invocationProvider.InvokeAsyncFunc<TResult>();
//            }
//            else
//            {
//                var methodInfo = typeof(TResult).GetMethod("InvokeAsync");
//                if (methodInfo != null)
//                {
//                    invocation = x => (Task<int>)methodInfo.Invoke(x, null);
//                }
//            }
//            if (invocation != null)
//            {
//                Func<InvocationContext, Task<int>> invocationWrapper
//                    = context => InvokeMethodWithResult(context, invocation);
//                command.Handler = new SimpleCommandHandler(invocationWrapper);
//            }
//        }

//        private Task<int> InvokeMethodWithResult<TResult>(InvocationContext context, Func<TResult, Task<int>> invocation)
//        {
//            var result = Activator.CreateInstance<TResult>();
//            var binder = new TypeBinder(typeof(TResult));
//            binder.SetProperties(context, result);
//            return invocation(result);
//        }

//        private async Task<int> InvokeAsync(InvocationContext x,
//            Func<Task<int>> invocation)
//        {
//            return await invocation();
//        }

//        private string GetHelp(Type currentType)
//        {
//            var attribute = currentType.GetCustomAttribute<HelpAttribute>(); ;
//            if (attribute != null)
//            {
//                return attribute.HelpText;
//            }
//            if (helpProvider != null)
//            {
//                return helpProvider.Description(currentType);
//            }

//            return "";
//        }

//        private string GetHelp(Type currentType, PropertyInfo propertyInfo)
//        {
//            var attribute = propertyInfo.GetCustomAttribute<HelpAttribute>();
//            if (attribute != null)
//            {
//                return attribute.HelpText;
//            }
//            if (helpProvider != null)
//            {
//                return helpProvider.Description(currentType, propertyInfo.Name);
//            }

//            return "";
//        }

//        private string GetHelp(Type currentType, ParameterInfo parameterInfo)
//        {
//            var attribute = parameterInfo.GetCustomAttribute<HelpAttribute>();
//            if (attribute != null)
//            {
//                return attribute.HelpText;
//            }
//            if (helpProvider != null)
//            {
//                return helpProvider.Description(currentType, parameterInfo.Name);
//            }

//            return "";
//        }

//        private string GetName(Type currentType)
//            => UnMungeName(currentType).ToLower();

//        private static string UnMungeName(Type currentType)
//        {
//            var name = currentType.Name;
//            var nameParts = PascalSplit(name);

//            var ancestors = new List<Type>();
//            while (currentType != typeof(object))
//            {
//                ancestors.Add(currentType);
//                currentType = currentType.BaseType;
//            }
//            ancestors.Reverse();
//            foreach (var ancestor in ancestors)
//            {
//                if (nameParts.Count() == 1)
//                {
//                    break;
//                }
//                if (String.Equals(nameParts.First(), ancestor.Name, StringComparison.InvariantCultureIgnoreCase))
//                {
//                    nameParts = nameParts.Skip(1);
//                }
//            }
//            return string.Join("", nameParts);
//        }

//        private static IEnumerable<string> PascalSplit(string name)
//        {
//            var parts = new List<string>();
//            var lastBreak = 0;
//            for (int i = 0; i < name.Length; i++)
//            {

//                if (Char.IsUpper(name, i) && i > lastBreak)
//                {
//                    parts.Add(name.Substring(lastBreak, i));
//                    lastBreak = i;
//                }
//            }
//            parts.Add(name.Substring(lastBreak));
//            return parts;
//        }
//    }
//}
