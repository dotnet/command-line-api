﻿using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    internal class MethodContext<TReturn>
    {
        public MethodContext(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        // for testing
        internal object Object { get; set; }

        public MethodInfo MethodInfo { get; }

        public async Task<TReturn> RunAsync(object[] values)
        {
            var retVal = MethodInfo.Invoke(Object, values);

            switch (retVal)
            {
                case Task<TReturn> taskOfT:
                    return await taskOfT;
                case Task task:
                    await task;
                    return default(TReturn);
                case TReturn value:
                    return value;
            }

            return default(TReturn);
        }
    }
}
