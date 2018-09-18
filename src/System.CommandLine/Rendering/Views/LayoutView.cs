using System.Collections.Generic;

namespace System.CommandLine.Rendering.Views
{
    public abstract class LayoutView<T> : View
        where T : View
    {
        //TODO: Only expose a readonly version of the children to make sure all adds flow through the AddChild method
        public IList<T> Children { get; } = new List<T>();

        public virtual void AddChild(T child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            Children.Add(child);
            
            child.Updated -= OnChildUpdated;
            child.Updated += OnChildUpdated;
        }

        protected virtual void OnChildUpdated(object sender, EventArgs e)
        {
            OnUpdated();
        }
    }
}
