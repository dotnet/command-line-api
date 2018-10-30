// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface IHelpBuilder
    {
        /// <summary>
        /// Writes help text for the provided <see cref="ICommand"/> to
        /// the configured <see cref="IConsole"/> instance
        /// </summary>
        /// <param name="command">
        /// The <see cref="ICommand"/> to generate and write text for
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        void Write(ICommand command);
    }
}
