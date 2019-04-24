using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace System.CommandLine
{
    internal static class Platform
    {
        private static readonly ConcurrentDictionary<string, bool> _unsupportedOperations =
            new ConcurrentDictionary<string, bool>();
        public static bool SupportsOperation<T>(Expression<Func<T>> apiCall, out T result)
        {
            if (apiCall == null)
            {
                throw new ArgumentNullException(nameof(apiCall));
            }

            if (!Unsupported(apiCall,  out var name))
            {
                result = default;
                return false;
            }

            try
            {
                var executor = apiCall.Compile();
                result = executor();
                return true;
            }

            catch (PlatformNotSupportedException)
            {
                _unsupportedOperations.TryAdd(name, false);
                result = default;
                return false;
            }
        }

        private static bool Unsupported<T>(Expression<T> apiCall, out string name) where T : Delegate
        {
            name = GetMemberName(apiCall.Body);
            return !_unsupportedOperations.TryGetValue(name, out _);
        }
     
        public static bool SupportsOperation(Expression<Action> apiCall)
        {
            if (apiCall == null)
            {
                throw new ArgumentNullException(nameof(apiCall));
            }
            if (!Unsupported(apiCall,  out var name))
            {
                return false;
            }
            try
            {
                var executor = apiCall.Compile();
                executor();
                return true;
            }

            catch (PlatformNotSupportedException)
            {
                _unsupportedOperations.TryAdd(name, false);

                return false;
            }
        }

        private static string GetMemberName(MemberExpression memberExpression)
        {
            return $"{memberExpression.Member.DeclaringType?.FullName ?? "_"}.{memberExpression.Member.Name}";
        }

        private static string GetMemberName(MethodCallExpression methodCallExpression)
        {
            return $"{methodCallExpression.Method.DeclaringType?.FullName ?? "_"}.{methodCallExpression.Method.Name}";
        }

        private static string GetMemberName(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    // Reference type property or field
                    return GetMemberName(memberExpression);
                case MethodCallExpression methodCallExpression:
                    // Reference type method
                    return GetMemberName(methodCallExpression);
                case UnaryExpression unaryExpression:
                    // Property, field of method returning value type
                    return GetMemberName(unaryExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.Operand)
            {
                case MethodCallExpression methodExpression:
                    return GetMemberName(methodExpression);
                case MemberExpression memberExpression:
                    return GetMemberName(memberExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryExpression));
            }
        }
    }
}
