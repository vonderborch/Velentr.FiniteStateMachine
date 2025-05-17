using System.Text.Json.Serialization;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents a concrete implementation of a state in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type of the state value.</typeparam>
public class State<TState> : IState<TState>
{
    [JsonIgnore] private readonly Dictionary<Func<object?, bool>, TState> functionalTransitions;

    [JsonIgnore] private readonly Dictionary<object, TState> transitions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="State{TState}" /> class with the specified state value.
    /// </summary>
    /// <param name="state">The value of the state.</param>
    public State(TState state)
    {
        this.StateValue = state;
        this.transitions = new Dictionary<object, TState>();
        this.functionalTransitions = new Dictionary<Func<object?, bool>, TState>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="State{TState}" /> class with the specified state value and
    ///     transitions.
    /// </summary>
    /// <param name="state">The value of the state.</param>
    /// <param name="transitions">The transitions available from this state.</param>
    [JsonConstructor]
    public State(TState state, Dictionary<object, TState> transitions)
    {
        this.StateValue = state;
        this.transitions = transitions;
    }

    /// <summary>
    ///     Gets the lambda (functional) transitions available from this state.
    /// </summary>
    [JsonIgnore]
    public Dictionary<Func<object?, bool>, TState> FunctionalTransitions => new(this.functionalTransitions);

    /// <summary>
    ///     Gets the value of the current state.
    /// </summary>
    [JsonPropertyName("state")]
    public TState StateValue { get; }

    /// <summary>
    ///     Gets the transitions available from this state.
    /// </summary>
    [JsonPropertyName("transitions")]
    public Dictionary<object, TState> Transitions => new(this.transitions);

    /// <summary>
    ///     Adds a transition to another state triggered by a specific event or condition.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> AddTransition(object trigger, TState nextState)
    {
        if (!this.transitions.ContainsKey(trigger))
        {
            this.transitions.Add(trigger, nextState);
        }

        return this;
    }

    /// <summary>
    ///     Adds a transition to another state triggered by a specific condition in blackboard data.
    /// </summary>
    /// <param name="triggerLambda">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> AddTransition(Func<object?, bool> triggerLambda, TState nextState)
    {
        if (!this.functionalTransitions.ContainsKey(triggerLambda))
        {
            this.functionalTransitions.Add(triggerLambda, nextState);
        }

        return this;
    }

    /// <summary>
    ///     Provides access to event handlers for state transitions in a finite state machine.
    /// </summary>
    [JsonIgnore]
    public StateEvents<TState>? Events { get; set; }

    /// <summary>
    ///     Registers an event handler for the Enter event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> RegisterOnEnterEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events == null)
        {
            this.Events = new StateEvents<TState>();
        }

        this.Events.OnEnter += handler;
        return this;
    }

    /// <summary>
    ///     Registers an event handler for the Exit event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> RegisterOnExitEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events == null)
        {
            this.Events = new StateEvents<TState>();
        }

        this.Events.OnExit += handler;
        return this;
    }

    /// <summary>
    ///     Registers an event handler for the Update event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> RegisterOnUpdateEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events == null)
        {
            this.Events = new StateEvents<TState>();
        }

        this.Events.OnUpdate += handler;
        return this;
    }

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="trigger">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> RemoveTransition(object trigger)
    {
        if (this.transitions.ContainsKey(trigger))
        {
            this.transitions.Remove(trigger);
        }

        return this;
    }

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="triggerLambda">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> RemoveTransition(Func<object?, bool> triggerLambda)
    {
        if (this.functionalTransitions.ContainsKey(triggerLambda))
        {
            this.functionalTransitions.Remove(triggerLambda);
        }

        return this;
    }

    /// <summary>
    ///     Determines whether a transition should occur based on the given trigger.
    /// </summary>
    /// <param name="trigger">The trigger to evaluate.</param>
    /// <param name="nextState">The next state if a transition should occur.</param>
    /// <returns>True if a transition should occur; otherwise, false.</returns>
    public bool ShouldTransitionFromTrigger(object trigger, out TState? nextState)
    {
        if (this.transitions.TryGetValue(trigger, out TState? state))
        {
            nextState = state;
            return true;
        }

        nextState = default;
        return false;
    }

    /// <summary>
    ///     Determines whether a transition should occur based on the current blackboard data.
    /// </summary>
    /// <param name="blackboard">The blackboard data context used to evaluate the transition.</param>
    /// <param name="nextState">The next state to transition to, if applicable.</param>
    /// <returns>True if a transition should occur; otherwise false.</returns>
    public bool ShouldTransitionFromUpdate(object? blackboard, out TState? nextState)
    {
        if (this.functionalTransitions.Count > 0)
        {
            foreach (KeyValuePair<Func<object?, bool>, TState> transition in this.functionalTransitions)
            {
                if (transition.Key(blackboard))
                {
                    nextState = transition.Value;
                    return true;
                }
            }
        }

        nextState = default;
        return false;
    }

    /// <summary>
    ///     Removes an event handler registered for the Enter event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> UnregisterOnEnterEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events != null)
        {
            this.Events.OnEnter -= handler;
        }

        return this;
    }

    /// <summary>
    ///     Removes an event handler registered for the Exit event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> UnregisterOnExitEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events != null)
        {
            this.Events.OnExit -= handler;
        }

        return this;
    }

    /// <summary>
    ///     Removes an event handler registered for the Update event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> UnregisterOnUpdateEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler)
    {
        if (this.Events != null)
        {
            this.Events.OnUpdate -= handler;
        }

        return this;
    }

    /// <summary>
    ///     Updates an existing transition to point to a new state.
    /// </summary>
    /// <param name="trigger">The trigger associated with the transition.</param>
    /// <param name="newNextState">The new state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> UpdateTransition(object trigger, TState newNextState)
    {
        if (this.transitions.ContainsKey(trigger))
        {
            this.transitions[trigger] = newNextState;
        }

        return this;
    }

    /// <summary>
    ///     Updates an existing transition to point to a new state.
    /// </summary>
    /// <param name="triggerLambda">The trigger associated with the transition.</param>
    /// <param name="newNextState">The new state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public IState<TState> UpdateTransition(Func<object?, bool> triggerLambda, TState newNextState)
    {
        if (this.functionalTransitions.ContainsKey(triggerLambda))
        {
            this.functionalTransitions[triggerLambda] = newNextState;
        }

        return this;
    }
}
