using System.Linq.Expressions;
using System.Reflection;

namespace System.CommandLine.Binding
{
    internal static class ExpressionExtensions
    {
        internal static (Type memberType, string memberName) MemberTypeAndName<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.Body is MemberExpression memberExpression)
            {
                return TypeAndName();
            }

            // when the return type of the expression is a value type, it contains a call to Convert, resulting in boxing, so we get a UnaryExpression instead
            if (expression.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;

                if (memberExpression != null)
                {
                    return TypeAndName();
                }
            }

            throw new ArgumentException($"Expression {expression} does not specify a member.");

            (Type memberType, string memberName) TypeAndName()
            {
                return (memberExpression.Member.ReturnType(), memberExpression.Member.Name);
            }
        }

        internal static Type ReturnType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                case MethodInfo methodInfo:
                    return methodInfo.ReturnType;
            }

            throw new InvalidOperationException($"Unexpected member type: {memberInfo}");
        }
    }
}
