namespace Velentr.FiniteStateMachine.Test;

[TestFixture]
public class TestState
{
    [Test]
    public void AddTransition_ShouldAddTransition()
    {
        State<string> state = new("State1");
        state.AddTransition("Trigger1", "State2");

        Assert.That(state.Transitions.ContainsKey("Trigger1"), Is.True);
        Assert.That(state.Transitions["Trigger1"], Is.EqualTo("State2"));
    }

    [Test]
    public void RemoveTransition_ShouldRemoveTransition()
    {
        State<string> state = new("State1");
        state.AddTransition("Trigger1", "State2");
        state.RemoveTransition("Trigger1");

        Assert.That(state.Transitions.ContainsKey("Trigger1"), Is.False);
    }

    [Test]
    public void ShouldTransitionFromTrigger_ShouldReturnTrueIfTransitionExists()
    {
        State<string> state = new("State1");
        state.AddTransition("Trigger1", "State2");

        var result = state.ShouldTransitionFromTrigger("Trigger1", out var nextState);

        Assert.That(result, Is.True);
        Assert.That(nextState, Is.EqualTo("State2"));
    }

    [Test]
    public void ShouldTransitionFromTrigger_ShouldReturnFalseIfTransitionDoesNotExist()
    {
        State<string> state = new("State1");

        var result = state.ShouldTransitionFromTrigger("Trigger1", out var nextState);

        Assert.That(result, Is.False);
        Assert.That(nextState, Is.Null);
    }

    [Test]
    public void RegisterOnEnterEvent_ShouldTriggerOnEnterEvent()
    {
        State<string> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnEnterEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnEnter.Trigger(this, new FiniteStateMachineEventArgs<string>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void RegisterOnExitEvent_ShouldTriggerOnExitEvent()
    {
        State<string> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnExitEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnExit.Trigger(this, new FiniteStateMachineEventArgs<string>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void RegisterOnUpdateEvent_ShouldTriggerOnUpdateEvent()
    {
        State<string> state = new("State1");
        var eventTriggered = false;

        state.RegisterOnUpdateEvent((sender, args) => { eventTriggered = true; });
        state.Events?.OnUpdate.Trigger(this, new FiniteStateMachineEventArgs<string>("State1", "State2", "Trigger1"));

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void FunctionalTransition_ShouldWorkCorrectly()
    {
        State<string> state = new("State1");
        state.AddTransition(blackboard => blackboard is int value && value > 10, "State2");

        var result = state.ShouldTransitionFromUpdate(15, out var nextState);

        Assert.That(result, Is.True);
        Assert.That(nextState, Is.EqualTo("State2"));
    }

    [Test]
    public void FunctionalTransition_ShouldReturnFalseIfConditionNotMet()
    {
        State<string> state = new("State1");
        state.AddTransition(blackboard => blackboard is int value && value > 10, "State2");

        var result = state.ShouldTransitionFromUpdate(5, out var nextState);

        Assert.That(result, Is.False);
        Assert.That(nextState, Is.Null);
    }
}
