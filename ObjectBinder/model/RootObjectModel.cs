using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class RootObjectModel : ObjectModelBase, IRootObjectModel
    {
        protected RootObjectModel( string cmdDescription = null )
        {
            var genericType = typeof(ObjectBinder<>);
            var objBinderType = genericType.MakeGenericType( GetType() );

            ObjectBinder = (IObjectBinder) Activator.CreateInstance( 
                objBinderType,
                new object[]
                {
                    this,
                    null,
                    null,
                    cmdDescription
                } );
        }

        public override bool Initialize( CommandLineBuilder builder, string[] args, IConsole console = null )
        {
            DefineAllBindings();

            base.Initialize( builder, args, console );

            ParseResult.Invoke(console);

            foreach (var childModel in ChildModels)
            {
                childModel.ParseResult.Invoke(console);
            }

            return ParseStatus.IsValid;
        }
    }
}