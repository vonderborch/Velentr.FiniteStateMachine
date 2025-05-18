namespace Velentr.FiniteStateMachine.Test;

[TestFixture]
public class TestState
{
    public class TestBlackboard(int value)
    {
        public int Value { get; set; } = value;
    }

    [Test]
    public void AddTransition_ShouldAddTransition()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition("Trigger1", "State2");

        Assert.That(state.Transitions.ContainsKey("Trigger1"), Is.True);
        Assert.That(state.Transitions["Trigger1"], Is.EqualTo("State2"));
    }

    [Test]
    public void RemoveTransition_ShouldRemoveTransition()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition("Trigger1", "State2");
        state.RemoveTransition("Trigger1");

        Assert.That(state.Transitions.ContainsKey("Trigger1"), Is.False);
    }

    [Test]
    public void ShouldTransitionFromTrigger_ShouldReturnTrueIfTransitionExists()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition("Trigger1", "State2");

        var result = state.ShouldTransitionFromTrigger("Trigger1", out var nextState);

        Assert.That(result, Is.True);
        Assert.That(nextState, Is.EqualTo("State2"));
    }

    [Test]
    public void ShouldTransitionFromTrigger_ShouldReturnFalseIfTransitionDoesNotExist()
    {
        State<string, string, TestBlackboard> state = new("State1");

        var result = state.ShouldTransitionFromTrigger("Trigger1", out var nextState);

        Assert.That(result, Is.False);
        Assert.That(nextState, Is.Null);
    }

    [Test]
    public void RegisterOnEnterEvent_ShouldTriggerOnEnterEvent()
    {
        State<string, string, TestBlackboard> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnEnterEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnEnter.Trigger(this,
            new FiniteStateMachineEventArgs<string, string, TestBlackboard>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void RegisterOnExitEvent_ShouldTriggerOnExitEvent()
    {
        State<string, string, TestBlackboard> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnExitEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnExit.Trigger(this,
            new FiniteStateMachineEventArgs<string, string, TestBlackboard>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void RegisterOnUpdateEvent_ShouldTriggerOnUpdateEvent()
    {
        State<string, string, TestBlackboard> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnUpdateEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnUpdate.Trigger(this,
            new FiniteStateMachineEventArgs<string, string, TestBlackboard>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void FunctionalTransition_ShouldWorkCorrectly()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition(blackboard => blackboard.Value > 10, "State2");

        var result = state.ShouldTransitionFromUpdate(new TestBlackboard(15), out var nextState);

        Assert.That(result, Is.True);
        Assert.That(nextState, Is.EqualTo("State2"));
    }

    [Test]
    public void FunctionalTransition_ShouldReturnFalseIfConditionNotMet()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition(blackboard => blackboard.Value > 10, "State2");

        var result = state.ShouldTransitionFromUpdate(new TestBlackboard(5), out var nextState);

        Assert.That(result, Is.False);
        Assert.That(nextState, Is.Null);
    }

    [Test]
    public void Serialize_ShouldReturnValidJson()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition("Trigger1", "State2");
        state.AddTransition(blackboard => blackboard.Value > 10, "State3");

        var serialized = state.Serialize();

        Assert.That(serialized, Is.Not.Null.And.Not.Empty);
        Assert.That(serialized, Does.Contain("State1"));
        Assert.That(serialized, Does.Contain("Trigger1"));
        Assert.That(serialized, Does.Contain("State2"));
        Assert.That(serialized, Does.Contain("State3"));
    }

    [Test]
    public void Deserialize_ShouldRecreateStateObject()
    {
        State<string, string, TestBlackboard> state = new("State1");
        state.AddTransition("Trigger1", "State2");
        state.AddTransition(blackboard => blackboard.Value > 10, "State3");

        var serialized = state.Serialize();
        State<string, string, TestBlackboard>? deserializedState =
            State<string, string, TestBlackboard>.Deserialize(serialized);

        Assert.That(deserializedState, Is.Not.Null);
        Assert.That(deserializedState!.StateValue, Is.EqualTo("State1"));
        Assert.That(deserializedState.Transitions.ContainsKey("Trigger1"), Is.True);
    }
}
