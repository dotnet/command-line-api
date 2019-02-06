// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Binding
{
    // This should be named Binder, but that name was taken at present
    public class BindingSet
    {
        private readonly List<Binding> _bindings = new List<Binding>();

        public void AddBinding(BindingSide targetSide, BindingSide parserSide)
            => _bindings.Add(new Binding(targetSide, parserSide));

        public void Bind(BindingContext context, object target)
        {
            foreach (var binding in _bindings)
            {
                binding.Bind(context, target);
            }
        }

        public IEnumerable<Binding> FindTargetBinding<T>(Func<T, bool> predicate)
            where T : BindingSide
            => _bindings
                .Where(b => b.TargetSide is T bs && predicate(bs));
    }
}
