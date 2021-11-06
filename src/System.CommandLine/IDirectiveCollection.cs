// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine
{
    /// <summary>
    /// A collection of directives parsed from a command line.
    /// </summary>
    /// <remarks>A directive is specified on the command line using square brackets, containing no spaces and preceding other tokens unless they are also directives. In the following example, two directives are present, <c>directive-one</c> and <c>directive-two</c>:
    /// <code>    > myapp [directive-one] [directive-two:value] arg1 arg2</code>
    /// The second has a value specified as well, <c>value</c>. Directive values can be read by calling using <see cref="TryGetValues"/>.
    /// </remarks>
    public interface IDirectiveCollection : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        /// <summary>
        /// Gets a value determining whether a directive with the specified name was parsed.
        /// </summary>
        /// <param name="name">The name of the directive.</param>
        /// <returns><see langword="true"/> if a directive with the specified name was parsed; otherwise, <see langword="false"/>.</returns>
        bool Contains(string name);

        /// <summary>
        /// Gets the values specified for a given directive. A return value indicates whether the specified directive name was present.
        /// </summary>
        /// <param name="name">The name of the directive.</param>
        /// <param name="values">The values provided for the specified directive.</param>
        /// <returns><see langword="true"/> if a directive with the specified name was parsed; otherwise, <see langword="false"/>.</returns>
        bool TryGetValues(string name,  [NotNullWhen(true)] out IReadOnlyList<string>? values);
    }
}
