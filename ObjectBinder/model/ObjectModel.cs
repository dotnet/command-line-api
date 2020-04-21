using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class ObjectModel : ObjectModelBase, IObjectModel
    {
        public ObjectModel( string cmdName, IRootObjectModel parentModel, string cmdDescription = null )
        {
            if( string.IsNullOrEmpty( cmdName ) )
                throw new ArgumentException( $"You must specify a {nameof(cmdName)}" );

            if( parentModel == null )
                throw new ArgumentException(
                    $"You must specify a parent {nameof(IObjectModel)}" );

            var genericType = typeof(ObjectBinder<>);
            var objBinderType = genericType.MakeGenericType( GetType() );

            ObjectBinder = (IObjectBinder) Activator.CreateInstance(
                objBinderType, this, cmdName, parentModel, cmdDescription );

            parentModel.Command.AddCommand( ObjectBinder.Command );
        }

        public void DefineBindings() => DefineBindings( ObjectBinder );
    }
}