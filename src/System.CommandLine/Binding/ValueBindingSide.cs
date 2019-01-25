// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    public class ValueBindingSide : BindingSide
    {
        private ValueBindingSide(BindingGetter getter, BindingSetter setter)
               : base(getter, setter)
        { }

        public static ValueBindingSide Create(Expression<Func<object>> valueExpression)
            => throw new NotImplementedException();

        public static ValueBindingSide Create<T>(Func<T> valueGetter, Action<object> valueSetter)
             => new ValueBindingSide((c, t) => valueGetter(), (c, t, value) => valueSetter(value));

        public static ValueBindingSide Create<T>(Func<T> valueGetter)
             => new ValueBindingSide((c, t) => valueGetter(), null);
    }
}
