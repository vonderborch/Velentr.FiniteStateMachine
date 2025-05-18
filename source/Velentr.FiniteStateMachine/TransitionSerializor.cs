using System.Linq.Expressions;
using Serialize.Linq.Serializers;
using JsonSerializer = Serialize.Linq.Serializers.JsonSerializer;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Provides serialization and deserialization capabilities for expressions.
/// </summary>
public static class TransitionSerializor<TBlackboard> where TBlackboard : class
{
    public static ExpressionSerializer Serializer = new(new JsonSerializer());

    /// <summary>
    ///     Deserializes a Base64 encoded string into an expression of type T.
    /// </summary>
    /// <param name="serializedExpression">The Base64 encoded string to deserialize.</param>
    /// <returns>The deserialized expression of type T, or null if the deserialization fails.</returns>
    public static Expression<Func<TBlackboard, bool>> DeserializeExpression(string serializedExpression)
    {
        Expression? expression = Serializer.DeserializeText(serializedExpression);
        return (Expression<Func<TBlackboard, bool>>)expression;
    }

    /// <summary>
    ///     Serializes an expression of type T into a Base64 encoded string.
    /// </summary>
    /// <param name="expression">The expression to serialize.</param>
    /// <returns>A Base64 encoded string representing the serialized expression.</returns>
    public static string SerializeExpression(Expression<Func<TBlackboard, bool>> expression)
    {
        var expressionString = Serializer.SerializeText(expression);
        return expressionString;
    }
}
