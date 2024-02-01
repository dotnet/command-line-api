// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Provides details about a command line completion item.
    /// </summary>
    public class CompletionItem : IEquatable<CompletionItem>
    {
        // reference: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/#textDocument_completion
        internal const string KindKeyword = "Keyword";
        internal const string KindValue = "Value";

        /// <param name="label">The label value, which is the text displayed to users and, unless <paramref name="insertText"/> is set, is also used to populate the <see cref="InsertText"/> property.</param>
        /// <param name="kind">The kind of completion item.</param>
        /// <param name="sortText">The value used to sort the completion item in a list. If this is not provided, then <paramref name="label"/>  is used.</param>
        /// <param name="insertText">The text to be inserted by this completion item. If this is not provided, then <paramref name="label"/>  is used.</param>
        /// <param name="documentation">Documentation about the completion item.</param>
        /// <param name="detail">Additional details regarding the completion item.</param>
        public CompletionItem(
            string label,
            string kind = CompletionItem.KindValue,
            string? sortText = null,
            string? insertText = null,
            string? documentation = null,
            string? detail = null)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Kind = kind ?? throw new ArgumentException(nameof(kind));
            SortText = sortText ?? label;
            InsertText = insertText ?? label;
            Documentation = documentation;
            Detail = detail;
        }

        /// <summary>
        /// The label value, which is the text displayed to users.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The kind of completion item.
        /// </summary>
        public string? Kind { get; }

        /// <summary>
        /// The value used to sort the completion item in a list.
        /// </summary>
        public string SortText { get; }

        /// <summary>
        /// The text to be inserted by this completion item.
        /// </summary>
        public string? InsertText { get; }

        /// <summary>
        /// Documentation about the completion item.
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Additional details regarding the completion item.
        /// </summary>
        public string? Detail { get; }

        /// <inheritdoc />
        public override string ToString() => Label;

        /// <summary>
        /// Determines whether two completion items are equal.
        /// </summary>
        public bool Equals(CompletionItem? other)
        {
            return other is not null && Label == other.Label && Kind == other.Kind;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CompletionItem);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Label.GetHashCode() * 397) ^ (Kind != null ? Kind.GetHashCode() : 0);
            }
        }
    }
}