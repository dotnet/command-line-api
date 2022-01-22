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
            while (source is not null)
            {
                yield return source;

                source = next(source);
            }
        }
    }
}