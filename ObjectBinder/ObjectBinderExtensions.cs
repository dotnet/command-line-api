using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace ObjectBinder
{
    public static class ObjectBinderExtensions
    {
        public static CommandLineBuilder UseObjectBinder( this CommandLineBuilder builder, IObjectBinder objBinder )
        {
            builder.AddMiddleware( async ( context, next ) =>
                {
                    var resultCode = await objBinder.Bind( context );
                    await next( context );
                },
                MiddlewareOrder.Default );

            return builder;
        }

        public static ObjectBinder<TClass> AddOption<TClass, TProp>(
            this ObjectBinder<TClass> objBinder,
            Expression<Func<TClass, TProp>> propSelector,
            Option<TProp> toAdd)
            where TClass : class
        {
            objBinder.ModelBinder.BindMemberFromValue(propSelector, toAdd);
            objBinder.Command.AddOption(toAdd);

            return objBinder;
        }

        public static Option<TProp> AddOption<TClass, TProp>(
            this ObjectBinder<TClass> objBinder,
            Expression<Func<TClass, TProp>> propSelector,
            params string[] aliases)
            where TClass : class
        {
            var toAdd = new Option<TProp>( aliases );

            objBinder.ModelBinder.BindMemberFromValue(propSelector, toAdd);
            objBinder.Command.AddOption(toAdd);

            return toAdd;
        }
    }
}