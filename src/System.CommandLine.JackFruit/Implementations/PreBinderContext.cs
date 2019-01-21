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
                // TODO : It will set defaults if you try to set values - thus there is no value to lateness
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
            SubCommandStrategies = (SubCommandStrategies ?? ProviderBase.Create<IEnumerable<Command>>())
                                        .AddStrategy<Type>(CommandProvider.FromDerivedTypes)
                                        .AddStrategy<Type>(CommandProvider.FromNestedTypes)
                                        .AddStrategy<Type>(CommandProvider.FromMethods);

            AliasStrategies = (AliasStrategies ?? ProviderBase.Create<IEnumerable<string>>())
                                        .SetFinalTransform((IEnumerable<string> x) => x.Select(n => n.ToKebabCase().ToLower()))
                                        .AddStrategy<object>(JackFruit.AliasProvider.FromAttribute);

            DescriptionStrategies = (DescriptionStrategies ?? ProviderBase.Create<string>())
                                        .AddStrategy<object>(JackFruit.DescriptionProvider.FromAttribute);

            OptionBindingStrategies = (OptionBindingStrategies ?? ProviderBase.Create<IEnumerable<(object Source, Option Option)>>())
                                         .AddStrategy<MethodInfo>(OptionProvider.FromParameters)
                                         .AddStrategy<Type>(OptionProvider.FromProperties);

            ArgumentBindingStrategies = (ArgumentBindingStrategies ?? ProviderBase.Create<IEnumerable<(object Source, Argument Argument)>>())
                                        .AddStrategy<Type>(ArgumentProvider.FromAttributedProperties)
                                        .AddStrategy<Type>(ArgumentProvider.FromSuffixedProperties)
                                        .AddStrategy<MethodInfo>(ArgumentProvider.FromAttributedParameters)
                                        .AddStrategy<MethodInfo>(ArgumentProvider.FromSuffixedParameters)
                                        .AddStrategy<ParameterInfo>(ArgumentProvider.FromParameter)
                                        .AddStrategy<PropertyInfo>(ArgumentProvider.FromProperty);

        }

        public IStrategySet<IEnumerable<Command>> SubCommandStrategies { get; set; }
        public IStrategySet<IEnumerable<string>> AliasStrategies { get; set; }
        public IStrategySet<string> DescriptionStrategies { get; set; }
        public IStrategySet<IEnumerable<(object Source, Argument Argument)>> ArgumentBindingStrategies { get; set; }
        public IStrategySet<IEnumerable<(object Source, Option Option)>> OptionBindingStrategies { get; set; }
    }
}
