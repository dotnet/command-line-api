// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Provides a set of <see langword="static"/> methods for querying objects that implement <see cref="IEnumerable"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> FlattenBreadthFirst<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> children)
        {
            var queue = new Queue<T>();

            foreach (var item in source)
            {
                queue.Enqueue(item);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var option in children(current))
                {
                    queue.Enqueue(option);
                }

                yield return current;
            }
        }

        internal static IEnumerable<T> RecurseWhileNotNull<T>(
            this T? source,
            Func<T, T?> next)
            where T : class
        {
            if (source is null) yield break;

            yield return source;

            while ((source = next(source)) != null)
            {
                yield return source;
            }
        }

        /// <summary>
        /// Determines whether any element of <paramref name="source"/> is of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to filter the elements of the <paramref name="source"/> on.</typeparam>
        /// <param name="source">The <see cref="IEnumerable"/> whose elements to filter.</param>
        /// <returns><see langword="true" /> if the source sequence contains any elements of type <typeparamref name="T"/>; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool HasAnyOfType<T>(this IEnumerable source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var obj in source)
            {
                if (obj is T)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the first <typeparamref name="T"/> element of <paramref name="source"/>, or a default value if no element is found.
        /// </summary>
        /// <typeparam name="T">The type to filter the elements of the <paramref name="source"/> on.</typeparam>
        /// <param name="source">The <see cref="IEnumerable"/> whose elements to filter.</param>
        /// <returnsThe first <typeparamref name="T"/> element of <paramref name="source"/>, if one exists; otherwise,the default value of <typeparamref name="T"/> <see cref="default{t}"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
#nullable disable
// requires C# 9.0
        public static T FirstOrDefaultOfType<T>(this IEnumerable source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var obj in source)
            {
                if (obj is T result)
                {
                    return result;
                }
            }

            return default(T);
        }
#nullable restore
    }
}
