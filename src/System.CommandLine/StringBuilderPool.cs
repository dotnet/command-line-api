// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Text;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// A pool of <see cref="StringBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Manges an arrray of <see cref="WeakReference{T}"/> of reusable instances of <see cref="StringBuilder"/>.
    /// </remarks>
    internal class StringBuilderPool
    {
        /// <summary>
        /// The default pool size.
        /// </summary>
        public const int DefaultPoolSize = 3;

        /// <summary>
        /// Gets the default instance of <see cref="StringBuilderPool"/>.
        /// </summary>
        /// <value>The default instance of <see cref="StringBuilderPool"/>.</value>
        public static StringBuilderPool Default { get; } = new();

        /// <summary>
        /// The pool of <see cref="WeakReference{T}"/> of reusable instances of <see cref="StringBuilder"/>.
        /// </summary>
        public readonly WeakReference<StringBuilder>?[] _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringBuilderPool"/> class.
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        public StringBuilderPool(int poolSize = DefaultPoolSize)
        {
            _pool = new WeakReference<StringBuilder>?[poolSize];
        }

        /// <summary>
        /// Gets a <see cref="StringBuilderPool"/> from the pool if one is available; otherwise, creates one.
        /// </summary>
        /// <returns>A <see cref="StringBuilderPool"/>.</returns>
        public StringBuilder Rent()
        {
            for (var i = _pool.Length; --i >= 0;)
            {
                if (Interlocked.Exchange(ref _pool[i], null) is { } builderReference && 
                    builderReference.TryGetTarget(out var builder))
                {
                    return builder.Clear();
                }
            }

            return new StringBuilder();
        }

        /// <summary>
        /// Returns a <see cref="StringBuilderPool"/> to the pool.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilderPool"/> to add to the pool.</param>
        /// <remarks>The <see cref="StringBuilderPool"/> doesn't need to be one returned from <see cref="Rent()"/>.</remarks>
        public void ReturnToPool(StringBuilder stringBuilder)
        {
            var reference = new WeakReference<StringBuilder>(stringBuilder);

            for (var i = _pool.Length; --i >= 0;)
            {
                if (Interlocked.CompareExchange(ref _pool[i], reference, null) is null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="string"/> from the <paramref name="stringBuilder"/> and returns it to the pool.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilderPool"/> to add to the pool.</param>
        /// <returns>The <see cref="string"/> created from the <paramref name="stringBuilder"/>.</returns>
        /// <remarks>The <paramref name="stringBuilder"/> doesn't need to be one returned from <see cref="Rent()"/>.</remarks>
        public string GetStringAndReturn(StringBuilder stringBuilder)
        {
            var text = stringBuilder.ToString();

            ReturnToPool(stringBuilder);

            return text;
        }
    }
}
