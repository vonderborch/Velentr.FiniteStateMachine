using System.Linq.Expressions;

namespace Velentr.FiniteStateMachine.Test;

[TestFixture]
public class TestTransitionSerializor
{
    public class TestBlackboard(int value)
    {
        public int Value { get; set; } = value;
    }

    [Test]
    public void DeserializeExpression_ShouldReturnOriginalExpression()
    {
        Expression<Func<TestBlackboard, bool>> expression = x => x != null;
        var serialized = TransitionSerializor<TestBlackboard>.SerializeExpression(expression);
        Expression<Func<TestBlackboard, bool>> deserialized =
            TransitionSerializor<TestBlackboard>.DeserializeExpression(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.ToString(), Is.EqualTo(expression.ToString()));
    }

    [Test]
    public void SerializeAndDeserialize_ShouldWorkForComplexExpressions()
    {
        Expression<Func<TestBlackboard, bool>> expression = x => x.Value > 10;
        var serialized = TransitionSerializor<TestBlackboard>.SerializeExpression(expression);
        Expression<Func<TestBlackboard, bool>> deserialized =
            TransitionSerializor<TestBlackboard>.DeserializeExpression(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.ToString(), Is.EqualTo(expression.ToString()));
    }
}
