// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class ContentSpan : TextSpan
    {
        public ContentSpan(string content)
        {
            Content = content ?? "";
        }

        public string Content { get; }

        public override int ContentLength => Content.Length;

        public override string ToString() => Content;

        protected bool Equals(ContentSpan other) => string.Equals(Content, other.Content);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() &&
                   Equals((ContentSpan)obj);
        }

        public override int GetHashCode() => Content?.GetHashCode() ?? 0;
    }
}
