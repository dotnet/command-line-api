// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides details about an item to be formatted to output in order to display two-column command line help.
    /// </summary>
    public class TwoColumnHelpRow : IEquatable<TwoColumnHelpRow?>
    {
        /// <param name="firstColumnText">The name and invocation details, typically displayed in the first help column.</param>
        /// <param name="secondColumnText">The description of a symbol, typically displayed in the second help column.</param>
        public TwoColumnHelpRow(string firstColumnText, string secondColumnText)
        {
            FirstColumnText = firstColumnText;
            SecondColumnText = secondColumnText;
        }

        /// <summary>
        /// The first column for a help entry, typically containing the name and other usage details about an option or argument.
        /// </summary>
        public string FirstColumnText { get; }

        /// <summary>
        /// The second column for a help entry, typically containing the name and other usage details about an option or argument.
        /// </summary>
        public string SecondColumnText { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TwoColumnHelpRow);
        }

        /// <inheritdoc />
        public bool Equals(TwoColumnHelpRow? other)
        {
            return other is not null &&
                   FirstColumnText == other.FirstColumnText &&
                   SecondColumnText == other.SecondColumnText;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = -244751520;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstColumnText);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondColumnText);
            return hashCode;
        }
    }
}
