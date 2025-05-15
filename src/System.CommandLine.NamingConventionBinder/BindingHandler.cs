// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace System.CommandLine.NamingConventionBinder
{
    /// <summary>
    /// Represents a handler that provides binding functionality for command-line actions.
    /// </summary>
    /// <remarks>This abstract class serves as a base for implementing custom binding logic in command-line
    /// applications. It provides a mechanism to retrieve or initialize a <see cref="BindingContext"/> for the current
    /// invocation.</remarks>
    public abstract class BindingHandler : AsynchronousCommandLineAction
    {
        private BindingContext? _bindingContext;

        /// <summary>
        /// The binding context for the current invocation.
        /// </summary>
        public virtual BindingContext GetBindingContext(ParseResult parseResult)
            => _bindingContext ??= new BindingContext(parseResult);
    }
}
