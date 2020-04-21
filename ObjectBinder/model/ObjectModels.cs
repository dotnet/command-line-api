using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class ObjectModels : KeyedCollection<string, IObjectModel>
    {
        protected override string GetKeyForItem( IObjectModel item ) => item.Command.Name;
    }
}