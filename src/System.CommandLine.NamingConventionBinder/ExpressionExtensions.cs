using System.Linq.Expressions;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

internal static class ExpressionExtensions
{
    internal static (Type memberType, string memberName) MemberTypeAndName<T, TValue>(this Expression<Func<T, TValue>> expression)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        if (expression.Body is MemberExpression memberExpression)
        {
            return TypeAndName(memberExpression);
        }

        // when the return type of the expression is a value type, it contains a call to Convert, resulting in boxing, so we get a UnaryExpression instead
        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression operandMemberExpression)
        {
            return TypeAndName(operandMemberExpression);
        }

        throw new ArgumentException($"Expression {expression} does not specify a member.");

        static (Type memberType, string memberName) TypeAndName(MemberExpression memberExpression)
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