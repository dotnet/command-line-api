// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;

namespace System.CommandLine.Binding
{

    public delegate void BindingSetter(InvocationContext context, object target, object value);
    public delegate object BindingGetter(InvocationContext context, object target);

    public class Binding
    {

        public BindingSide TargetSide { get; }
        public BindingSide ParserSide { get; }

        public Binding(BindingSide targetSide, BindingSide parserSide)
        {
            TargetSide = targetSide;
            ParserSide = parserSide;
        }

        public void BindDefaults(InvocationContext context = null, object target = null)
        {
            var value = TargetSide.Get(context, target);
            ParserSide.Set(context, target, value);
        }

        public void Bind(InvocationContext context = null, object target = null)
        {
            var value = ParserSide.Get(context, target);
            TargetSide.Set(context, target, value);
        }


    }
}
