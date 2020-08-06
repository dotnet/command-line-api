﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
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
    }
}
