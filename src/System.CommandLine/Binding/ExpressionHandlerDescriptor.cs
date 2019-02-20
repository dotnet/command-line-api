// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    internal class ExpressionHandlerDescriptor : HandlerDescriptor
    {
        private readonly LambdaExpression _handlerExpression;

        public ExpressionHandlerDescriptor(LambdaExpression handlerExpression)
        {
            _handlerExpression = handlerExpression;
        }

        public override ICommandHandler GetCommandHandler()
        {
            // TODO: (GetCommandHandler) 
            return null;
        }

        protected override IEnumerable<ParameterDescriptor> InitializeParameterDescriptors()
        {
            switch (_handlerExpression.Body)
            {
                case MethodCallExpression methodCall:
                    foreach (var p in methodCall.Method.GetParameters())
                    {
                        yield return new ParameterDescriptor(p);
                    }

                    yield break;

                default:
                    throw new NotSupportedException($"Unsupported expression: {_handlerExpression}");
            }
        }
    }
}
