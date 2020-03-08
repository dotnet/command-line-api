// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public abstract class View
    {
        public event EventHandler Updated;

        public abstract void Render(ConsoleRenderer renderer, Region region = null);

        public abstract Size Measure(ConsoleRenderer renderer, Size maxSize);

        protected void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    }
}
