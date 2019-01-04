using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class PreBinderContext
    {
        private static PreBinderContext current;
        public static PreBinderContext Current
        {
            get
            {
                if (current == null)
                {
                    current = new PreBinderContext();
                }
                return current;
            }
        }
        private IFinder<IEnumerable<Command>> subCommandFinder;
        public IFinder<IEnumerable<Command>> SubCommandFinder
        {
            get => subCommandFinder ?? (subCommandFinder = FinderBase.Create<IEnumerable<Command>>()
                                                           .AddApproach<Type>(CommandStrategies.FromDerivedTypes)
                                                           .AddApproach<Type>(CommandStrategies.FromNestedTypes)
                                                           .AddApproach<Type>(CommandStrategies.FromMethods));
            set => subCommandFinder = value;
        }

        private IFinder<IEnumerable<string>> aliasFinder;
        public IFinder<IEnumerable<string>> AliasFinder
        {
            get => aliasFinder ?? (aliasFinder = FinderBase.Create<IEnumerable<string>>()
                                                            .SetFinalTransform(x => x.Select(n => n.ToKebabCase().ToLower()))
                                                            .AddApproach<object>(AliasStrategies.FromAttribute));
            set => aliasFinder = value;
        }

        private IFinder<string> descriptionFinder;
        public IFinder<string> DescriptionFinder
        {
            get => descriptionFinder ?? (descriptionFinder = FinderBase.Create<string>()
                                                            .AddApproach<object>(DescriptionStrategies.FromAttribute));
            set => descriptionFinder = value;
        }

        private IFinder<IEnumerable<Argument>> argumentFinder;
        public IFinder<IEnumerable<Argument>> ArgumentFinder
        {
            get => argumentFinder ?? (argumentFinder = FinderBase.Create<IEnumerable<Argument>>()
                                                            .AddApproach<Type>(ArgumentStrategies.FromAttributedProperties)
                                                            .AddApproach<Type>(ArgumentStrategies.FromSuffixedProperties)
                                                            .AddApproach<MethodInfo>(ArgumentStrategies.FromAttributedParameters)
                                                            .AddApproach<MethodInfo>(ArgumentStrategies.FromSuffixedParameters)
                                                            .AddApproach<ParameterInfo>(ArgumentStrategies.FromParameter)
                                                            .AddApproach<PropertyInfo>(ArgumentStrategies.FromProperty));
            set => argumentFinder = value;
        }

        private IFinder<IEnumerable<Option>> optionFinder;
        public IFinder<IEnumerable<Option>> OptionFinder
        {
            get => optionFinder ?? (optionFinder = FinderBase.Create<IEnumerable<Option>>()
                                                            .AddApproach<Type>(OptionStrategies.FromProperties)
                                                            .AddApproach<MethodInfo>(OptionStrategies.FromParameters));
            set => optionFinder = value;
        }

        private IFinder<ICommandHandler> handlerFinder;
        public IFinder<ICommandHandler> HandlerFinder
        {
            get => handlerFinder ?? (handlerFinder = FinderBase.Create<ICommandHandler>()
                                                            .AddApproach<MethodInfo>(HandlerStrategies.FromMethod)
                                                            .AddApproach<Type>(HandlerStrategies.FromInvokeOnType));
            set => handlerFinder = value;
        }

    }
}
