using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class ObjectModelBase
    {
        protected ObjectModelBase()
        {
        }

        public IObjectBinder ObjectBinder { get; protected set; }

        public ParseResult ParseResult { get; protected set; }
        public ParseStatus ParseStatus { get; protected set; }
        public Command Command => ObjectBinder.Command;

        public ObjectModels ChildModels { get; } = new ObjectModels();

        // by default don't look for help or version flags, as that should be
        // taken care of in the RootObjectModel
        protected virtual CommandLineBuilder GetCommandLineBuilder() =>
            new CommandLineBuilder(ObjectBinder.Command)
                .UseDefaults()
                .UseObjectBinder(ObjectBinder);

        protected virtual void DefineBindings( IObjectBinder objBinder )
        {
        }

        protected virtual void DefineAllBindings()
        {
            DefineBindings( ObjectBinder );

            foreach (var childModel in ChildModels)
            {
                childModel.DefineBindings();
            }
        }

        protected string[] ExtractRelevantTokens( 
            CommandLineConfiguration cmdLineConfig,
            string[] args )
        {
            var readOnly = new ReadOnlyCollection<string>( args );
            var tokenized = readOnly.Tokenize( cmdLineConfig );

            if( tokenized.Errors.Count > 0 )
                throw new InvalidOperationException(
                    $"Couldn't parse command line arguments for command '{Command.Name}'" );

            // first token appears to contain the command we're tokenizing for
            var cmdName = tokenized.Tokens.First().Value;

            var retVal = new List<string>();

            var extract = false;
            var completed = false;

            // scan all the tokens but only extract them for parsing when 
            // they relate to our command. There are two ways that is signified.
            // For RootCommands there will be a Command type token whose value
            // matches cmdName (it appears to always be the first token).
            // For regular Commands there will be an Argument type token whose
            // value matches cmdName. Note that for regular Commands we skip
            // the first token because it's not relevant
            foreach( var token in tokenized.Tokens.Skip(ObjectBinder.Command is RootCommand ? 0 : 1) )
            {
                switch( token.Type )
                {
                    case TokenType.Argument:
                        // if we're already extracting the token is related to
                        // an option that should have already been captured
                        if( extract )
                            retVal.Add( token.Value );
                        else
                        {
                            // if the argument value matches the cmdName start extracting
                            if( token.Value.Equals( cmdName, StringComparison.OrdinalIgnoreCase ) )
                            {
                                extract = true;
                                retVal.Add( token.Value );
                            }
                        }

                        break;

                    case TokenType.Command:
                        if( token.Value.Equals( Command.Name, StringComparison.OrdinalIgnoreCase ) )
                        {
                            extract = true;
                            retVal.Add( token.Value );
                        }
                        else completed = true;

                        break;

                    case TokenType.Option:
                    case TokenType.Operand:
                        if( extract )
                            retVal.Add( token.Value );

                        break;

                    case TokenType.Directive:
                        // no op; we don't handle directives yet so ignore them
                        break;

                    case TokenType.EndOfArguments:
                        extract = false;
                        completed = true;

                        break;
                }

                if( completed ) break;
            }

            return retVal.ToArray();
        }

        public virtual bool Initialize(string[] args, IConsole console = null) =>
            Initialize(GetCommandLineBuilder(), args, console);

        public virtual bool Initialize(string args, IConsole console = null)
        {
            var splitter = CommandLineStringSplitter.Instance;

            var splitArgs = splitter.Split(args).ToArray();

            return Initialize(GetCommandLineBuilder(), splitArgs, console);
        }

        public virtual bool Initialize( CommandLineBuilder builder, string[] args, IConsole console = null )
        {
            var parser = builder.Build();

            ParseResult = parser.Parse( ExtractRelevantTokens( parser.Configuration, args ) );

            ParseStatus = new ParseStatus()
            {
                HelpRequested = ParseResult.FindResultFor(builder.HelpOption) != null,
                FoundErrors = ParseResult.Errors.Count > 0
            };

            foreach (var childModel in ChildModels)
            {
                var result = childModel.Initialize(args, console);

                ParseStatus.FoundErrors |= childModel.ParseStatus.FoundErrors;
                ParseStatus.HelpRequested |= childModel.ParseStatus.HelpRequested;
            }

            return ParseStatus.IsValid;
        }
    }
}