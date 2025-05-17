namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents the event arguments for state transitions in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type representing the states in the finite state machine.</typeparam>
/// <param name="fromState">The state the machine is transitioning from.</param>
/// <param name="toState">The state the machine is transitioning to.</param>
/// <param name="trigger">The trigger that caused the state transition.</param>
/// <param name="blackboard">Optional shared data associated with the transition.</param>
/// <param name="isReset">Indicates whether the transition is a reset operation.</param>
public class FiniteStateMachineEventArgs<TState>(
    TState fromState,
    TState toState,
    object trigger,
    object? blackboard = null,
    bool isReset = false)
    : EventArgs
{
    /// <summary>
    ///     Gets or sets the optional shared data associated with the transition.
    /// </summary>
    public object? Blackboard { get; set; } = blackboard;

    /// <summary>
    ///     Gets or sets the state the machine is transitioning from.
    /// </summary>
    public TState FromState { get; set; } = fromState;

    /// <summary>
    ///     Gets or sets a value indicating whether the transition is a reset operation.
    /// </summary>
    public bool IsReset { get; set; } = isReset;

    /// <summary>
    ///     Gets or sets the state the machine is transitioning to.
    /// </summary>
    public TState ToState { get; set; } = toState;

    /// <summary>
    ///     Gets or sets the trigger that caused the state transition.
    /// </summary>
    public object Trigger { get; set; } = trigger;
}
