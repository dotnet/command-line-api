// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    public abstract class HandlerDescriptor
    {
        private List<ParameterDescriptor> _parameterDescriptors;

        public abstract ICommandHandler GetCommandHandler();

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors =>
            _parameterDescriptors ?? (_parameterDescriptors = new List<ParameterDescriptor>(InitializeParameterDescriptors()));

        protected abstract IEnumerable<ParameterDescriptor> InitializeParameterDescriptors();

        public static HandlerDescriptor FromExpression<TModel>(Expression<Action<TModel>> handle) => new ExpressionHandlerDescriptor(handle);

        public static HandlerDescriptor FromExpression<TModel, T>(Expression<Action<TModel, T>> handle) => new ExpressionHandlerDescriptor(handle);
        public static HandlerDescriptor FromExpression<TModel, T1, T2>(Expression<Action<TModel, T1, T2>> handle) => new ExpressionHandlerDescriptor(handle);
        public static HandlerDescriptor FromExpression<TModel, T1, T2, TReturn>(Expression<Func<TModel, T1, T2, TReturn>> handle) => new ExpressionHandlerDescriptor(handle);
    }
}
