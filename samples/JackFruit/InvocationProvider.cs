using System;
using System.CommandLine.JackFruit;
using System.Threading.Tasks;

namespace JackFruit
{
    internal class InvocationProvider : IInvocationProvider
    {
        public Func<T, Task<int>> InvokeAsyncFunc<T>()
        {
            if (typeof(T) == typeof(Sln.Add))
            {
                return x =>
                {
                    var add = x as Sln.Add;
                    return SlnActions.AddAsync(add.SlnFile, add.ProjectFile);
                };
            }
            if (typeof(T) == typeof(Sln.Remove))
            {
                return x =>
                {
                    var add = x as Sln.Add;
                    return SlnActions.RemoveAsync(add.SlnFile, add.ProjectFile);
                };
            }
            if (typeof(T) == typeof(Sln.List))
            {
                return x =>
                {
                    var add = x as Sln.List;
                    return SlnActions.ListAsync(add.SlnFile);
                };
            }
            return null;
        }
    }
}
