// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

using System.Linq.Expressions;
using System.Reflection;

namespace NSubstitute;

/// <summary>
/// Provides extension methods to assist with testing non-public members using NSubstitute.
/// </summary>
public static class NSubstituteExtensions
{
    /// <summary>
    /// Invokes a protected method on a target instance based on an expression specifying the method.
    /// </summary>
    /// <typeparam name="T">The type of the target object on which the method should be invoked.</typeparam>
    /// <typeparam name="TResult">The return type of the method being invoked.</typeparam>
    /// <param name="target">The target object on which the method should be invoked.</param>
    /// <param name="methodExpression">An expression that specifies the method to be invoked.</param>
    /// <param name="args">Arguments to pass to the method being invoked.</param>
    /// <returns>The result of the method invocation, or the default value of <typeparamref name="TResult"/> if the method has no return value.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided expression is not a method call.</exception>
    /// <exception cref="Exception">Thrown when the method is not a virtual member.</exception>
    /// <example>
    /// This is how you can use the <c>Protected</c> method with an expression:
    /// <code>
    /// var fooInstance = new Foo();
    /// var result = fooInstance.Protected(f => f.SomeMethod(0), 5);
    /// Console.WriteLine(result);
    /// </code>
    /// </example>
    public static TResult? Protected<T, TResult>(this T target, Expression<Func<T, TResult>> methodExpression, params object[] args)
    {
        if (methodExpression.Body is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("The provided expression is not a method call.");
        }

        var method = methodCall.Method;
        var type = target?.GetType();

        if (method.DeclaringType != type)
        {
            method = type?.GetMethod(method.Name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        return method != null && !method.IsVirtual
            ? throw new Exception("The method must be a virtual member.")
            : (TResult?)method?.Invoke(target, args);
    }

    /// <summary>
    /// Invokes a protected method on a target instance based on the method name.
    /// </summary>
    /// <param name="target">The target object on which the method should be invoked.</param>
    /// <param name="methodName">The name of the method to be invoked.</param>
    /// <param name="args">Arguments to pass to the method being invoked.</param>
    /// <returns>The result of the method invocation, or null if the method has no return value.</returns>
    /// <exception cref="Exception">Thrown when the method is not a virtual member.</exception>
    /// <example>
    /// This is how you can use the <c>Protected</c> method with a method name:
    /// <code>
    /// var fooInstance = new Foo();
    /// var result = fooInstance.Protected("SomeMethod", 5);
    /// Console.WriteLine(result);
    /// </code>
    /// And here's an example using NSubstitute to verify calls on protected methods:
    /// <code>
    /// var sub = Substitute.For&lt;Foo&gt;();
    /// sub.ReallyDoStuff(5);
    /// sub.Received().Protected("DoStuff", 5);
    /// sub.DidNotReceive().Protected("DoStuff", 2);
    /// </code>    
    /// </example>
    public static object? Protected(this object target, string methodName, params object[] args)
    {
        var type = target.GetType();
        var method = type
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == methodName);

        return !method.IsVirtual ? throw new Exception("The method must be a virtual member.") : method.Invoke(target, args);
    }
}