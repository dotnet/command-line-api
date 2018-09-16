using System.Collections.Generic;

namespace System.CommandLine.Rendering.Views
{
    public abstract class LayoutView<T> : View
        where T : View
    {
        public IList<T> Children { get; } = new List<T>();

        public virtual void AddChild(T child)
        {
            Children.Add(child);
            child.Updated += HandleChildUpdate;
        }

        protected virtual void HandleChildUpdate(object sender, EventArgs e)
        {
        }
    }
}
