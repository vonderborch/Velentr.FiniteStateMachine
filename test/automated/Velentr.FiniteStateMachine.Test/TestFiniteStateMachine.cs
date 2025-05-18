using System.Text.Json;

namespace Velentr.FiniteStateMachine.Test;

[TestFixture]
public class TestFiniteStateMachine
{
    public enum States
    {
        State1,
        State2,
        State3
    }

    public class TestBlackboard(int value)
    {
        public int Value { get; set; } = value;
    }

    [Test]
    public void AddState_ShouldAddState()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State2);
        Assert.That(fsm.ContainsState(States.State1), Is.False);
        Assert.That(fsm.ContainsState(States.State2), Is.True);
        Assert.That(fsm.ContainsState(States.State3), Is.False);
    }

    [Test]
    public void AddTransition_ShouldAddTransition()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm =
            new FiniteStateMachine<States, string, TestBlackboard>(States.State1)
                .AddState(States.State1)
                .AddState(States.State2)
                .AddTransition(States.State1, States.State2, "Trigger1");

        Assert.That(fsm.Trigger("Trigger1", out var transitioned), Is.EqualTo(States.State2));
        Assert.That(transitioned, Is.True);
    }

    [Test]
    public void RemoveState_ShouldRemoveState()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State2).RemoveState(States.State2);

        Assert.That(fsm.ContainsState(States.State1), Is.False);
        Assert.That(fsm.ContainsState(States.State2), Is.False);
        Assert.That(fsm.ContainsState(States.State3), Is.False);
    }

    [Test]
    public void Reset_ShouldReturnToStartingState()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, "Trigger1");

        fsm.Trigger("Trigger1");
        Assert.That(fsm.Reset(), Is.True);
    }

    [Test]
    public void Update_ShouldTransitionBasedOnBlackboard()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard.Value > 10);

        Assert.That(fsm.Update(new TestBlackboard(15)), Is.True);
    }

    [Test]
    public void Trigger_ShouldNotTransitionIfNoMatch()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        Assert.That(fsm.Trigger("InvalidTrigger", out var transitioned), Is.EqualTo(States.State1));
        Assert.That(transitioned, Is.False);
    }

    [Test]
    public void Trigger_ShouldReturnCorrectPreviousAndNextState()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, "Trigger1");

        fsm.Trigger("Trigger1", out States previousState, out States nextState);

        Assert.That(previousState, Is.EqualTo(States.State1));
        Assert.That(nextState, Is.EqualTo(States.State2));
    }

    [Test]
    public void Update_ShouldNotTransitionIfNoConditionMet()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard.Value > 10);

        Assert.That(fsm.Update(new TestBlackboard(5)), Is.False);
    }

    [Test]
    public void AddTransition_WithLambda_ShouldWorkCorrectly()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard.Value == 123);

        Assert.That(fsm.Update(new TestBlackboard(123)), Is.True);
    }

    [Test]
    public void ValidateContainsAllStates_ShouldReturnTrueWhenAllStatesArePresent()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddState(States.State3);

        var result = fsm.ValidateContainsAllStates();

        Assert.That(result, Is.True);
    }

    [Test]
    public void ValidateContainsAllStates_ShouldReturnFalseWhenStatesAreMissing()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        var result = fsm.ValidateContainsAllStates();

        Assert.That(result, Is.False);
    }

    [Test]
    public void ValidateContainsAllStates_WithOutParameter_ShouldReturnTrueWhenAllStatesArePresent()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddState(States.State3);

        FiniteStateMachine<States, string, TestBlackboard> result =
            fsm.ValidateContainsAllStates(out var hasMissingStates);

        Assert.That(hasMissingStates, Is.True);
        Assert.That(result, Is.EqualTo(fsm));
    }

    [Test]
    public void ValidateContainsAllStates_WithOutParameter_ShouldReturnFalseWhenStatesAreMissing()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        FiniteStateMachine<States, string, TestBlackboard> result =
            fsm.ValidateContainsAllStates(out var hasMissingStates);

        Assert.That(hasMissingStates, Is.False);
        Assert.That(result, Is.EqualTo(fsm));
    }

    [Test]
    public void Serialize_ShouldReturnValidJson()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddTransition(States.State1, States.State2, "Trigger1");

        var serialized = fsm.Serialize();

        Assert.That(serialized, Is.Not.Null.And.Not.Empty);
        Assert.That(serialized, Does.Contain("State1"));
        Assert.That(serialized, Does.Contain("State2"));
        Assert.That(serialized, Does.Contain("Trigger1"));
    }

    [Test]
    public void Deserialize_ShouldRecreateFiniteStateMachine()
    {
        FiniteStateMachine<States, string, TestBlackboard> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddTransition(States.State1, States.State2, "Trigger1");

        var serialized = fsm.Serialize();
        FiniteStateMachine<States, string, TestBlackboard>? deserializedFsm =
            FiniteStateMachine<States, string, TestBlackboard>.Deserialize(serialized);

        Assert.That(deserializedFsm, Is.Not.Null);
        Assert.That(deserializedFsm!.StartingStateValue, Is.EqualTo(States.State1));
        Assert.That(deserializedFsm.ContainsState(States.State1), Is.True);
        Assert.That(deserializedFsm.ContainsState(States.State2), Is.True);
        Assert.That(deserializedFsm.Trigger("Trigger1", out var transitioned), Is.EqualTo(States.State2));
        Assert.That(transitioned, Is.True);
    }

    [Test]
    public void Deserialize_ShouldThrowForInvalidJson()
    {
        var invalidJson = "{ invalid json }";

        Assert.Throws<JsonException>(() =>
            FiniteStateMachine<States, string, TestBlackboard>.Deserialize(invalidJson));
    }
}
