namespace Markwardt;

public static class Delegator
{
    public delegate object? Implementation(IReadOnlyDictionary<ParameterInfo, object?> arguments);

    private static readonly Type argumentsType = typeof(Dictionary<ParameterInfo, object?>);
    private static readonly Lazy<ConstructorInfo> argumentsConstructor = new(() => argumentsType.GetConstructor(Type.EmptyTypes).NotNull());
    private static readonly Lazy<MethodInfo> argumentsAddMethod = new(() => argumentsType.GetMethod(nameof(IDictionary.Add)).NotNull());

    public static Delegate CreateDelegate(Type delegateType, Expression<Implementation> implementation)
    {
        MethodInfo invoke = delegateType.GetMethod(nameof(Action.Invoke)).NotNull();

        List<(ParameterInfo Info, ParameterExpression Expression)> parameters = invoke.GetParameters().Select(p => (p, Expression.Parameter(p.ParameterType, p.Name))).ToList();
        List<(ParameterInfo Info, ParameterExpression Expression)> injectedParameters = parameters;

        ParameterExpression arguments = Expression.Variable(argumentsType, nameof(arguments));
        ParameterExpression result = Expression.Variable(typeof(object), nameof(result));

        IEnumerable<Expression> body =
        [
            Expression.Assign(arguments, Expression.New(argumentsConstructor.Value)),
            .. injectedParameters.Select(p => Expression.Call(arguments, argumentsAddMethod.Value, Expression.Constant(p.Info), Expression.Convert(p.Expression, typeof(object)))),
            Expression.Assign(result, Expression.Invoke(implementation, arguments)),
            invoke.ReturnType == typeof(void) ? result : Expression.Convert(result, invoke.ReturnType)
        ];

        return Expression.Lambda(delegateType, Expression.Block(invoke.ReturnType, [arguments, result], body), parameters.Select(x => x.Expression)).Compile();
    }

    public static T CreateDelegate<T>(Expression<Implementation> implementation)
        where T : Delegate
        => (T)CreateDelegate(typeof(T), implementation);
}