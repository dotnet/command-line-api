// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{

    public delegate void BindingSetter(BindingContext context, object target, object value);
    public delegate object BindingGetter(BindingContext context, object target);

    public class Binding
    {
        public BindingSide TargetSide { get; }
        public BindingSide ParserSide { get; }

        public Binding(BindingSide targetSide, BindingSide parserSide)
        {
            TargetSide = targetSide ??
                         throw new ArgumentNullException(nameof(targetSide));
            ParserSide = parserSide ??
                         throw new ArgumentNullException(nameof(parserSide));
        }

        public void BindDefaults(BindingContext context, object target = null)
        {
            var value = TargetSide.Get(context, target);
            ParserSide.Set(context, target, value);
        }

        public void Bind(BindingContext context, object target = null)
        {
            var value = ParserSide.Get(context, target);
            TargetSide.Set(context, target, value);
        }
    }
}
