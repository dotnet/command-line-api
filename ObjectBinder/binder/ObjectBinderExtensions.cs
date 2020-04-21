using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public static class ObjectBinderExtensions
    {
        public static CommandLineBuilder UseObjectBinder( this CommandLineBuilder builder, IObjectBinder objBinder )
        {
            builder.UseMiddleware( async ( context, next ) =>
            {
                var resultCode = await objBinder.Bind( context );
                await next( context );
            } );

            return builder;
        }

        public static ObjectBinder<TModel> AddOption<TModel, TProp>(
            this ObjectBinder<TModel> objBinder,
            Expression<Func<TModel, TProp>> propSelector,
            Option<TProp> toAdd)
            where TModel : class, IRootObjectModel
        {
            objBinder.ModelBinder.BindMemberFromValue(propSelector, toAdd);
            objBinder.Command.AddOption(toAdd);

            return objBinder;
        }

        public static Option<TProp> AddOption<TModel, TProp>(
            this ObjectBinder<TModel> objBinder,
            Expression<Func<TModel, TProp>> propSelector,
            params string[] aliases)
            where TModel : class, IRootObjectModel
        {
            var toAdd = new Option<TProp>( aliases );

            objBinder.ModelBinder.BindMemberFromValue(propSelector, toAdd);
            objBinder.Command.AddOption(toAdd);

            return toAdd;
        }
    }
}