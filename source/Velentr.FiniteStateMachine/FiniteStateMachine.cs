using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents a finite state machine that manages states and transitions between them.
/// </summary>
/// <typeparam name="TState">The type representing the states in the finite state machine.</typeparam>
[DebuggerDisplay("")]
public class FiniteStateMachine<TState> where TState : notnull
{
    private TState currentState;

    private readonly Dictionary<TState, IState<TState>?> states;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FiniteStateMachine{TState}" /> class.
    /// </summary>
    /// <param name="startingState">The initial state of the finite state machine.</param>
    /// <param name="currentState">The current state of the finite state machine.</param>
    [JsonConstructor]
    public FiniteStateMachine(TState startingState, TState? currentState = default)
    {
        this.StartingStateValue = startingState;
        this.currentState = currentState ?? startingState;
        this.states = new Dictionary<TState, IState<TState>?>();
    }

    /// <summary>
    ///     Gets the value of the starting state of the finite state machine.
    /// </summary>
    [JsonPropertyName("startingState")]
    public TState StartingStateValue { get; }
    
    /// <summary>
    ///     Gets the value of the current state of the finite state machine.
    /// </summary>
    [JsonPropertyName("currentState")]
    public TState CurrentStateValue => this.currentState;
    
    /// <summary>
    ///     Gets the current state of the finite state machine.
    /// </summary>
    [JsonIgnore]
    public IState<TState> CurrentState => this.states[this.currentState]!;
    
    /// <summary>
    ///     Gets the starting state of the finite state machine.
    /// </summary>
    [JsonIgnore]
    public IState<TState> StartingState => this.states[this.StartingStateValue]!;

    /// <summary>
    ///     Adds a state with a custom state object to the finite state machine.
    /// </summary>
    /// <param name="state">The state to add.</param>
    /// <param name="stateObject">The custom state object.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> AddIState(TState state, IState<TState> stateObject)
    {
        this.states.TryAdd(state, stateObject);
        return this;
    }

    /// <summary>
    ///     Adds a state to the finite state machine.
    /// </summary>
    /// <param name="state">The state to add.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> AddState(TState state)
    {
        return AddIState(state, new State<TState>(state));
    }

    /// <summary>
    ///     Adds a transition between two states triggered by a specific event.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="to">The state to transition to.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> AddTransition(TState from, TState to, object trigger)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(to))
        {
            this.states[from]?.AddTransition(trigger, to);
        }

        return this;
    }

    /// <summary>
    ///     Adds a transition between two states triggered by a condition.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="to">The state to transition to.</param>
    /// <param name="triggerLambda">The condition that causes the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> AddTransition(TState from, TState to, Func<object?, bool> triggerLambda)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(to))
        {
            this.states[from]?.AddTransition(triggerLambda, to);
        }

        return this;
    }

    /// <summary>
    ///     Checks if the finite state machine contains a specific state.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>True if the state exists; otherwise, false.</returns>
    public bool ContainsState(TState state)
    {
        return this.states.ContainsKey(state);
    }

    /// <summary>
    ///     Removes a state from the finite state machine.
    /// </summary>
    /// <param name="state">The state to remove.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> RemoveState(TState state)
    {
        if (this.states.ContainsKey(state))
        {
            this.states.Remove(state);
        }

        return this;
    }

    /// <summary>
    ///     Removes a transition between two states triggered by a specific event.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="trigger">The trigger associated with the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> RemoveTransition(TState from, object trigger)
    {
        if (this.states.ContainsKey(from))
        {
            this.states[from]?.RemoveTransition(trigger);
        }

        return this;
    }

    /// <summary>
    ///     Removes a transition between two states triggered by a condition.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="triggerLambda">The condition associated with the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> RemoveTransition(TState from, Func<object?, bool> triggerLambda)
    {
        if (this.states.ContainsKey(from))
        {
            this.states[from]?.RemoveTransition(triggerLambda);
        }

        return this;
    }

    /// <summary>
    ///     Resets the finite state machine to its starting state.
    /// </summary>
    /// <param name="blackboard">Optional shared data associated with the reset.</param>
    /// <returns>True if the reset was successful; otherwise, false.</returns>
    public bool Reset(object? blackboard = null)
    {
        var transitionResult = Transition(this.currentState, this.StartingStateValue, null, blackboard, true);
        return transitionResult;
    }

    /// <summary>
    ///     Sets the current state of the finite state machine.
    /// </summary>
    /// <param name="state">The state to set as the current state.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> SetState(TState state)
    {
        if (this.states.ContainsKey(state))
        {
            this.currentState = state;
        }

        return this;
    }

    private bool Transition(TState from, TState to, object trigger, object? blackboard = null, bool isReset = false)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(to) && !Equals(this.currentState, to))
        {
            IState<TState>? current = this.states[from];
            IState<TState>? next = this.states[to];

            if (current!.Events != null)
            {
                current.Events.OnExit.Trigger(this,
                    new FiniteStateMachineEventArgs<TState>(from, to, trigger, blackboard, isReset));
            }

            if (next!.Events != null)
            {
                next.Events.OnEnter.Trigger(this,
                    new FiniteStateMachineEventArgs<TState>(from, to, trigger, blackboard, isReset));
            }

            this.currentState = to;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="blackboard">Optional shared data associated with the transition.</param>
    /// <returns>True if the transition was successful; otherwise, false.</returns>
    public bool Trigger(object trigger, object? blackboard = null)
    {
        if (this.states[this.currentState]!.ShouldTransitionFromTrigger(trigger, out TState? nextState))
        {
            var transitionResult = Transition(this.currentState, nextState!, trigger, blackboard);
            return transitionResult;
        }

        return false;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="transitioned">Outputs True if a transition occurred, False otherwise.</param>
    /// <param name="blackboard">Optional shared data associated with the transition.</param>
    /// <returns>The current state of the Finite State Machine.</returns>
    public TState Trigger(object trigger, out bool transitioned, object? blackboard = null)
    {
        transitioned = false;
        if (this.states[this.currentState]!.ShouldTransitionFromTrigger(trigger, out TState? nextState))
        {
            transitioned = Transition(this.currentState, nextState!, trigger, blackboard);
        }

        return this.currentState;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="blackboard">Optional shared data associated with the transition.</param>
    /// <returns>True if the transition was successful; otherwise, false.</returns>
    public bool Trigger(object trigger, out TState startingState, out TState? nextState, object? blackboard = null)
    {
        var transitioned = false;
        startingState = this.currentState;
        if (this.states[this.currentState]!.ShouldTransitionFromTrigger(trigger, out nextState))
        {
            transitioned = Transition(this.currentState, nextState!, trigger, blackboard);
        }

        nextState = transitioned ? nextState : default;
        return transitioned;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="blackboard">Optional shared data associated with the update.</param>
    /// <returns>True if a transition occurred; otherwise, false.</returns>
    public bool Update(object? blackboard = null)
    {
        if (this.states[this.currentState]!.ShouldTransitionFromUpdate(blackboard, out TState? nextState))
        {
            var transitioned = Transition(this.currentState, nextState!, null, blackboard);
            return transitioned;
        }

        return false;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="blackboard">Optional shared data associated with the update.</param>
    /// <returns>True if a transition occurred; otherwise, false.</returns>
    public bool Update(out TState startingState, out TState? nextState, object? blackboard = null)
    {
        startingState = this.currentState;
        var transitioned = Update(blackboard);
        nextState = transitioned ? this.currentState : default;
        return transitioned;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="transitioned">Outputs True if a transition occurred, False otherwise.</param>
    /// <param name="blackboard">Optional shared data associated with the update.</param>
    public void Update(out TState startingState, out TState? nextState, out bool transitioned,
        object? blackboard = null)
    {
        startingState = this.currentState;
        transitioned = Update(blackboard);
        nextState = transitioned ? this.currentState : default;
    }

    /// <summary>
    ///     Validates whether all possible states are present in the finite state machine.
    /// </summary>
    /// <returns>True if all states are present; otherwise, false.</returns>
    public bool ValidateContainsAllStates()
    {
        Array possibleStates = Enum.GetValues(typeof(TState));
        foreach (var state in possibleStates)
        {
            if (!this.states.ContainsKey((TState)state))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Validates whether all possible states are present in the finite state machine and provides the result.
    /// </summary>
    /// <param name="hasMissingStates">Outputs whether any states are missing.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState> ValidateContainsAllStates(out bool hasMissingStates)
    {
        hasMissingStates = ValidateContainsAllStates();
        return this;
    }
}
