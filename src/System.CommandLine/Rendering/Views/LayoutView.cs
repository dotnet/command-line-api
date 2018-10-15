using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering.Views
{
    //TODO: consider IEnumerable<T> addition
    public abstract class LayoutView<T> : View
        where T : View
    {
        private readonly List<T> _children = new List<T>();
        public IReadOnlyList<T> Children => _children.AsReadOnly();

        public virtual void AddChild(T child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);
            
            child.Updated -= OnChildUpdated;
            child.Updated += OnChildUpdated;
        }

        public virtual void ClearChildren()
        {
            while (Children.Any())
            {
                RemoveChild(Children[0]);
            }
        }

        public virtual bool RemoveChild(T child)
        {
            child.Updated -= OnChildUpdated;
            return _children.Remove(child);
        }

        protected virtual void OnChildUpdated(object sender, EventArgs e)
        {
            OnUpdated();
        }
    }
}
