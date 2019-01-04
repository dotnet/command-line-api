using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class PreBinderContext
    {
        private static PreBinderContext current;
        public static void Empty()
        {
            current = new PreBinderContext();
        }
        public static PreBinderContext Current
        {
            get
            {
                if (current == null)
                {
                    current = new PreBinderContext();
                    current.SetDefaults();
                }
                return current;
            }
        }

        private void SetDefaults()
        {
            SubCommandProvider = (SubCommandProvider ?? ProviderBase.Create<IEnumerable<Command>>())
                                                    .AddStrategy<Type>(CommandStrategies.FromDerivedTypes)
                                                    .AddStrategy<Type>(CommandStrategies.FromNestedTypes)
                                                    .AddStrategy<Type>(CommandStrategies.FromMethods);

            AliasProvider = (AliasProvider ?? ProviderBase.Create<IEnumerable<string>>())
                                                    .SetFinalTransform(x => x.Select(n => n.ToKebabCase().ToLower()))
                                                    .AddStrategy<object>(AliasStrategies.FromAttribute);

            DescriptionProvider = (DescriptionProvider ?? ProviderBase.Create<string>())
                                                    .AddStrategy<object>(DescriptionStrategies.FromAttribute);

            ArgumentProvider = (ArgumentProvider ?? ProviderBase.Create<IEnumerable<Argument>>())
                                                    .AddStrategy<Type>(ArgumentStrategies.FromAttributedProperties)
                                                    .AddStrategy<Type>(ArgumentStrategies.FromSuffixedProperties)
                                                    .AddStrategy<MethodInfo>(ArgumentStrategies.FromAttributedParameters)
                                                    .AddStrategy<MethodInfo>(ArgumentStrategies.FromSuffixedParameters)
                                                    .AddStrategy<ParameterInfo>(ArgumentStrategies.FromParameter)
                                                    .AddStrategy<PropertyInfo>(ArgumentStrategies.FromProperty);

            OptionProvider = (OptionProvider ?? ProviderBase.Create<IEnumerable<Option>>())
                                                    .AddStrategy<Type>(OptionStrategies.FromProperties)
                                                    .AddStrategy<MethodInfo>(OptionStrategies.FromParameters);

            HandlerProvider = (HandlerProvider ?? ProviderBase.Create<ICommandHandler>())
                                                    .AddStrategy<MethodInfo>(HandlerStrategies.FromMethod)
                                                    .AddStrategy<Type>(HandlerStrategies.FromInvokeOnType);
        }

        public IProvider<IEnumerable<Command>> SubCommandProvider { get; set; }
        public IProvider<IEnumerable<string>> AliasProvider { get; set; }
        public IProvider<string> DescriptionProvider { get; set; }
        public IProvider<IEnumerable<Argument>> ArgumentProvider { get; set; }
        public IProvider<IEnumerable<Option>> OptionProvider { get; set; }
        public IProvider<ICommandHandler> HandlerProvider { get; set; }
    }
}
