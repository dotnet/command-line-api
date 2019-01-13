﻿using System.Collections.Generic;
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

            ChildProvider = (ChildProvider ?? ProviderBase.Create<IEnumerable<ISymbolBase>>())
                                                    .AddStrategy<MethodInfo>(ChildStrategies.FromMethod)
                                                    .AddStrategy<Type>(ChildStrategies.FromType);

            OptionBindingActionProvider = (OptionBindingActionProvider ?? ProviderBase.Create<IEnumerable<SymbolBinding >>())
                                         .AddStrategy<MethodInfo>(OptionStrategies2.FromParameters)
                                         .AddStrategy<Type>(OptionStrategies2.FromProperties);
        }

        public IProvider<IEnumerable<Command>> SubCommandProvider { get; set; }
        public IProvider<IEnumerable<string>> AliasProvider { get; set; }
        public IProvider<string> DescriptionProvider { get; set; }
        public IProvider<IEnumerable<Argument>> ArgumentProvider { get; set; }
        public IProvider<IEnumerable<Option>> OptionProvider { get; set; }
        public IProvider<IEnumerable<SymbolBinding>> OptionBindingActionProvider { get; set; }
        public IProvider<ICommandHandler> HandlerProvider { get; set; }
        public IProvider<IEnumerable<ISymbolBase>> ChildProvider { get; set; }
    }
}
