namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents a state in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type of the state value.</typeparam>
public interface IState<TState>
{
    /// <summary>
    ///     Gets the value of the current state.
    /// </summary>
    TState StateValue { get; }

    /// <summary>
    ///     Gets the transitions available from this state.
    /// </summary>
    Dictionary<object, TState> Transitions { get; }

    /// <summary>
    ///     Provides access to event handlers for state transitions in a finite state machine.
    /// </summary>
    StateEvents<TState>? Events { get; set; }

    /// <summary>
    ///     Adds a transition to another state triggered by a specific event or condition.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> AddTransition(object trigger, TState nextState);

    /// <summary>
    ///     Adds a transition to another state triggered by a specific condition in blackboard data.
    /// </summary>
    /// <param name="triggerLambda">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> AddTransition(Func<object?, bool> triggerLambda, TState nextState);

    /// <summary>
    ///     Registers an event handler for the Enter event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> RegisterOnEnterEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Registers an event handler for the Exit event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> RegisterOnExitEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Registers an event handler for the Update event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> RegisterOnUpdateEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="trigger">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> RemoveTransition(object trigger);

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="triggerLambda">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> RemoveTransition(Func<object?, bool> triggerLambda);

    /// <summary>
    ///     Determines whether a transition should occur based on the given trigger.
    /// </summary>
    /// <param name="trigger">The trigger to evaluate.</param>
    /// <param name="nextState">The next state if a transition should occur.</param>
    /// <returns>True if a transition should occur; otherwise, false.</returns>
    bool ShouldTransitionFromTrigger(object trigger, out TState? nextState);

    /// <summary>
    ///     Determines whether a transition should occur based on the current blackboard data.
    /// </summary>
    /// <param name="blackboard">The blackboard data context used to evaluate the transition.</param>
    /// <param name="nextState">The next state to transition to, if applicable.</param>
    /// <returns>True if a transition should occur; otherwise false.</returns>
    bool ShouldTransitionFromUpdate(object? blackboard, out TState? nextState);

    /// <summary>
    ///     Removes an event handler registered for the Enter event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> UnregisterOnEnterEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Removes an event handler registered for the Exit event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> UnregisterOnExitEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Removes an event handler registered for the Update event of the state.
    /// </summary>
    /// <param name="handler">The event handler to be removed.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> UnregisterOnUpdateEvent(EventHandler<FiniteStateMachineEventArgs<TState>> handler);

    /// <summary>
    ///     Updates an existing transition to point to a new state.
    /// </summary>
    /// <param name="trigger">The trigger associated with the transition.</param>
    /// <param name="newNextState">The new state to transition to.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> UpdateTransition(object trigger, TState newNextState);

    /// <summary>
    ///     Updates an existing transition to point to a new state.
    /// </summary>
    /// <param name="triggerLambda">The trigger associated with the transition.</param>
    /// <param name="newNextState">The new state to transition to.</param>
    /// <returns>The current state instance.</returns>
    IState<TState> UpdateTransition(Func<object?, bool> triggerLambda, TState newNextState);
}
