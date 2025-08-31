namespace Markwardt;

public static class Delegator
{
    public delegate object? Implementation(IReadOnlyDictionary<string, object?> arguments);

    public static Delegate CreateDelegate(Type delegateType, Expression<Implementation> implementation)
    {
        MethodInfo invoke = delegateType.GetMethod(nameof(Action.Invoke)).NotNull();
        IReadOnlyList<ParameterExpression> parameters = invoke.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();
        ConstantExpression arguments = Expression.Constant(new Dictionary<string, object?>());
        ParameterExpression result = Expression.Variable(typeof(object));

        Type argumentsType = typeof(Dictionary<string, object?>);
        MethodInfo addMethod = argumentsType.GetMethod(nameof(IDictionary.Add)).NotNull();
        MethodInfo clearMethod = argumentsType.GetMethod(nameof(IDictionary.Clear)).NotNull();

        IEnumerable<Expression> body =
        [
            .. parameters.Select(p => Expression.Call(arguments, addMethod, Expression.Constant(p.Name), Expression.Convert(p, typeof(object)))),
            Expression.Assign(result, Expression.Invoke(implementation, arguments)),
            Expression.Call(arguments, clearMethod),
            invoke.ReturnType == typeof(void) ? result : Expression.Convert(result, invoke.ReturnType)
        ];

        return Expression.Lambda(delegateType, Expression.Block(invoke.ReturnType, [result], body), parameters).Compile();
    }

    public static T CreateDelegate<T>(Expression<Implementation> implementation)
        where T : Delegate
        => (T)CreateDelegate(typeof(T), implementation);
}