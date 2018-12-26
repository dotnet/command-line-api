using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class HandlerFinder : FinderBase<ICommandHandler>
    {
        public HandlerFinder(params Approach<ICommandHandler>[] approaches)
            : base(approaches: approaches)
        { }

        private static (bool, ICommandHandler) FromMethod(Command parent, MethodInfo method)
            => method != null
                  ? (true, CommandHandler.Create(method))
                  : (false, null);

        private static (bool, ICommandHandler) FromInvokeOnType(Command parent, Type type)
        {
            var invokeMethod = type.GetMethod("InvokeAsync");
            return type != null && invokeMethod != null
                             ? (true, CommandHandler.Create(invokeMethod))
                             : (false, null);
        }

        public static Approach<ICommandHandler> MethodApproach()
             => Approach<ICommandHandler>.CreateApproach<object>(
                 (p,x)=>FromMethod(p,x as MethodInfo));

        public static Approach<ICommandHandler> InvokeOnTypeApproach()
             => Approach<ICommandHandler>.CreateApproach<object>(
                 (p,x)=> FromInvokeOnType(p,x as Type));

        public static HandlerFinder Default() 
            => new HandlerFinder(MethodApproach(), InvokeOnTypeApproach());

    }
}
