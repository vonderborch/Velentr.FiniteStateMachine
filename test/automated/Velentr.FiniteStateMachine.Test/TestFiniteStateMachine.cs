namespace Velentr.FiniteStateMachine.Test;

[TestFixture]
public class TestFiniteStateMachine
{
    private enum States
    {
        State1,
        State2,
        State3
    }

    [Test]
    public void AddState_ShouldAddState()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State2);
        Assert.That(fsm.ContainsState(States.State1), Is.False);
        Assert.That(fsm.ContainsState(States.State2), Is.True);
        Assert.That(fsm.ContainsState(States.State3), Is.False);
    }

    [Test]
    public void AddTransition_ShouldAddTransition()
    {
        FiniteStateMachine<States> fsm = new FiniteStateMachine<States>(States.State1)
            .AddState(States.State1)
            .AddState(States.State2)
            .AddTransition(States.State1, States.State2, "Trigger1");

        Assert.That(fsm.Trigger("Trigger1", out var transitioned), Is.EqualTo(States.State2));
        Assert.That(transitioned, Is.True);
    }

    [Test]
    public void RemoveState_ShouldRemoveState()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State2).RemoveState(States.State2);

        Assert.That(fsm.ContainsState(States.State1), Is.False);
        Assert.That(fsm.ContainsState(States.State2), Is.False);
        Assert.That(fsm.ContainsState(States.State3), Is.False);
    }

    [Test]
    public void Reset_ShouldReturnToStartingState()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, "Trigger1");

        fsm.Trigger("Trigger1");
        Assert.That(fsm.Reset(), Is.True);
    }

    [Test]
    public void Update_ShouldTransitionBasedOnBlackboard()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard is int value && value > 10);

        Assert.That(fsm.Update(15), Is.True);
    }

    [Test]
    public void Trigger_ShouldNotTransitionIfNoMatch()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        Assert.That(fsm.Trigger("InvalidTrigger", out var transitioned), Is.EqualTo(States.State1));
        Assert.That(transitioned, Is.False);
    }

    [Test]
    public void Trigger_ShouldReturnCorrectPreviousAndNextState()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, "Trigger1");

        fsm.Trigger("Trigger1", out States previousState, out States nextState);

        Assert.That(previousState, Is.EqualTo(States.State1));
        Assert.That(nextState, Is.EqualTo(States.State2));
    }

    [Test]
    public void Update_ShouldNotTransitionIfNoConditionMet()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard is int value && value > 10);

        Assert.That(fsm.Update(5), Is.False);
    }

    [Test]
    public void AddTransition_WithLambda_ShouldWorkCorrectly()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);
        fsm.AddTransition(States.State1, States.State2, blackboard => blackboard is string str && str == "Go");

        Assert.That(fsm.Update("Go"), Is.True);
    }

    [Test]
    public void ValidateContainsAllStates_ShouldReturnTrueWhenAllStatesArePresent()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddState(States.State3);

        var result = fsm.ValidateContainsAllStates();

        Assert.That(result, Is.True);
    }

    [Test]
    public void ValidateContainsAllStates_ShouldReturnFalseWhenStatesAreMissing()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        var result = fsm.ValidateContainsAllStates();

        Assert.That(result, Is.False);
    }

    [Test]
    public void ValidateContainsAllStates_WithOutParameter_ShouldReturnTrueWhenAllStatesArePresent()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2).AddState(States.State3);

        FiniteStateMachine<States> result = fsm.ValidateContainsAllStates(out var hasMissingStates);

        Assert.That(hasMissingStates, Is.True);
        Assert.That(result, Is.EqualTo(fsm));
    }

    [Test]
    public void ValidateContainsAllStates_WithOutParameter_ShouldReturnFalseWhenStatesAreMissing()
    {
        FiniteStateMachine<States> fsm = new(States.State1);
        fsm.AddState(States.State1).AddState(States.State2);

        FiniteStateMachine<States> result = fsm.ValidateContainsAllStates(out var hasMissingStates);

        Assert.That(hasMissingStates, Is.False);
        Assert.That(result, Is.EqualTo(fsm));
    }
}
