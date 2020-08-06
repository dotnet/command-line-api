// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public interface ITableViewColumn<T>
    {
        ColumnDefinition ColumnDefinition { get; }

        View Header { get; }

        View GetCell(T item, TextSpanFormatter formatter);
    }
}
