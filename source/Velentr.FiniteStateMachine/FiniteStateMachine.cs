using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents a finite state machine that manages states and transitions between them.
/// </summary>
/// <typeparam name="TState">The type representing the states in the finite state machine.</typeparam>
/// <typeparam name="TTrigger">The type representing the triggers that cause transitions.</typeparam>
/// <typeparam name="TBlackboard">The type representing the shared data associated with the finite state machine.</typeparam>
[DebuggerDisplay("Current State: {CurrentStateValue} (Valid: {ValidateFiniteStateMachine()})")]
public class FiniteStateMachine<TState, TTrigger, TBlackboard>
    where TState : notnull
    where TTrigger : IEquatable<TTrigger>
    where TBlackboard : class
{
    [JsonIgnore] private readonly Dictionary<TState, State<TState, TTrigger, TBlackboard>?> states;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FiniteStateMachine{TState, TTrigger, TBlackboard}" /> class.
    /// </summary>
    /// <param name="startingState">The initial state of the finite state machine.</param>
    public FiniteStateMachine(TState startingState)
    {
        this.StartingStateValue = startingState;
        this.CurrentStateValue = startingState;
        this.states = new Dictionary<TState, State<TState, TTrigger, TBlackboard>?>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FiniteStateMachine{TState, TTrigger, TBlackboard}" /> class.
    /// </summary>
    /// <param name="startingStateValue">The initial state of the finite state machine.</param>
    /// <param name="currentStateValue">The current state of the finite state machine.</param>
    /// <param name="states">The states of the finite state machine.</param>
    [JsonConstructor]
    public FiniteStateMachine(TState startingStateValue, TState currentStateValue,
        Dictionary<TState, State<TState, TTrigger, TBlackboard>?> states)
    {
        this.StartingStateValue = startingStateValue;
        this.CurrentStateValue = currentStateValue;
        this.states = states;
    }

    /// <summary>
    ///     Gets the current state of the finite state machine.
    /// </summary>
    [JsonIgnore]
    public State<TState, TTrigger, TBlackboard> CurrentState => this.states[this.CurrentStateValue]!;

    /// <summary>
    ///     Gets the starting state of the finite state machine.
    /// </summary>
    [JsonIgnore]
    public State<TState, TTrigger, TBlackboard> StartingState => this.states[this.StartingStateValue]!;

    /// <summary>
    ///     Gets the value of the starting state of the finite state machine.
    /// </summary>
    [JsonPropertyName("startingStateValue")]
    public TState StartingStateValue { get; }

    /// <summary>
    ///     Gets the collection of states in the finite state machine, mapped by their state values.
    /// </summary>
    [JsonPropertyName("states")]
    public Dictionary<TState, State<TState, TTrigger, TBlackboard>?> States => new(this.states);

    /// <summary>
    ///     Gets the value of the current state of the finite state machine.
    /// </summary>
    [JsonPropertyName("currentStateValue")]
    [field: JsonIgnore]
    public TState CurrentStateValue { get; private set; }

    /// <summary>
    ///     Adds a state with a custom state object to the finite state machine.
    /// </summary>
    /// <param name="state">The state to add.</param>
    /// <param name="stateObject">The custom state object.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> AddState(TState state,
        State<TState, TTrigger, TBlackboard> stateObject)
    {
        this.states.TryAdd(state, stateObject);
        return this;
    }

    /// <summary>
    ///     Adds a state to the finite state machine.
    /// </summary>
    /// <param name="state">The state to add.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> AddState(TState state)
    {
        return AddState(state, new State<TState, TTrigger, TBlackboard>(state));
    }

    /// <summary>
    ///     Adds a transition between two states triggered by a specific event.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="to">The state to transition to.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> AddTransition(TState from, TState to, TTrigger trigger)
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
    public FiniteStateMachine<TState, TTrigger, TBlackboard> AddTransition(TState from, TState to,
        Expression<Func<TBlackboard, bool>> triggerLambda)
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
    ///     Deserializes an FSM from a serialized string representation.
    /// </summary>
    /// <param name="serializedState">The serialized string representation of the FSM.</param>
    /// <param name="options">Optional JSON serializer options for customization.</param>
    /// <returns>The current FSM instance after deserialization.</returns>
    public static FiniteStateMachine<TState, TTrigger, TBlackboard>? Deserialize(string serializedState,
        JsonSerializerOptions? options = null)
    {
        options ??= Constants.SerializationOptions;
        FiniteStateMachine<TState, TTrigger, TBlackboard>? fsm =
            JsonSerializer.Deserialize<FiniteStateMachine<TState, TTrigger, TBlackboard>>(serializedState, options);
        return fsm;
    }

    /// <summary>
    ///     Removes a state from the finite state machine.
    /// </summary>
    /// <param name="state">The state to remove.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> RemoveState(TState state)
    {
        this.states.Remove(state);

        return this;
    }

    /// <summary>
    ///     Removes a transition between two states triggered by a specific event.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="trigger">The trigger associated with the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> RemoveTransition(TState from, TTrigger trigger)
    {
        if (this.states.TryGetValue(from, out State<TState, TTrigger, TBlackboard>? state))
        {
            state?.RemoveTransition(trigger);
        }

        return this;
    }

    /// <summary>
    ///     Removes a transition between two states triggered by a condition.
    /// </summary>
    /// <param name="from">The state to transition from.</param>
    /// <param name="triggerLambda">The condition associated with the transition.</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> RemoveTransition(TState from,
        Expression<Func<TBlackboard, bool>> triggerLambda)
    {
        if (this.states.TryGetValue(from, out State<TState, TTrigger, TBlackboard>? state))
        {
            state?.RemoveTransition(triggerLambda);
        }

        return this;
    }

    /// <summary>
    ///     Resets the finite state machine to its starting state.
    /// </summary>
    /// <param name="blackboard">Shared data associated with the reset.</param>
    /// <returns>True if the reset was successful; otherwise, false.</returns>
    public bool Reset(TBlackboard? blackboard = null)
    {
        var transitionResult = Transition(this.CurrentStateValue, this.StartingStateValue, default, blackboard, true);
        return transitionResult;
    }

    /// <summary>
    ///     Serializes the current FSM into a string representation.
    /// </summary>
    /// <param name="options">Optional JSON serializer options for customization.</param>
    /// <returns>A string containing the serialized FSM data.</returns>
    public string Serialize(JsonSerializerOptions? options = null)
    {
        options ??= Constants.SerializationOptions;
        var json = JsonSerializer.Serialize(this, options);
        return json;
    }

    /// <summary>
    ///     Serializes the finite state machine instance into a JSON string.
    /// </summary>
    /// <param name="fsm">The finite state machine to serialize.</param>
    /// <param name="options">Optional serializer options for customizing the serialization process.</param>
    /// <returns>A JSON string representation of the finite state machine.</returns>
    public static string Serialize(FiniteStateMachine<TState, TTrigger, TBlackboard> fsm,
        JsonSerializerOptions? options = null)
    {
        return fsm.Serialize(options);
    }

    private bool Transition(TState from, TState to, TTrigger? trigger, TBlackboard? blackboard, bool isReset = false)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(to) && !Equals(this.CurrentStateValue, to))
        {
            State<TState, TTrigger, TBlackboard>? current = this.states[from];
            State<TState, TTrigger, TBlackboard>? next = this.states[to];

            if (current!.Events != null)
            {
                current.Events.OnExit.Trigger(this,
                    new FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>(from, to, trigger, blackboard,
                        isReset));
            }

            if (next!.Events != null)
            {
                next.Events.OnEnter.Trigger(this,
                    new FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>(from, to, trigger, blackboard,
                        isReset));
            }

            this.CurrentStateValue = to;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="blackboard">Shared data associated with the transition.</param>
    /// <returns>True if the transition was successful; otherwise, false.</returns>
    public bool Trigger(TTrigger trigger, TBlackboard? blackboard = null)
    {
        if (this.states[this.CurrentStateValue]!.ShouldTransitionFromTrigger(trigger, out TState nextState))
        {
            var transitionResult = Transition(this.CurrentStateValue, nextState, trigger, blackboard);
            return transitionResult;
        }

        return false;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="transitioned">Outputs True if a transition occurred, False otherwise.</param>
    /// <param name="blackboard">Shared data associated with the transition.</param>
    /// <returns>The current state of the Finite State Machine.</returns>
    public TState Trigger(TTrigger trigger, out bool transitioned, TBlackboard? blackboard = null)
    {
        transitioned = false;
        if (this.states[this.CurrentStateValue]!.ShouldTransitionFromTrigger(trigger, out TState nextState))
        {
            transitioned = Transition(this.CurrentStateValue, nextState, trigger, blackboard);
        }

        return this.CurrentStateValue;
    }

    /// <summary>
    ///     Triggers a transition based on a specific event.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="blackboard">Shared data associated with the transition.</param>
    /// <returns>True if the transition was successful; otherwise, false.</returns>
    public bool Trigger(TTrigger trigger, out TState startingState, out TState? nextState,
        TBlackboard? blackboard = null)
    {
        var transitioned = false;
        startingState = this.CurrentStateValue;
        if (this.states[this.CurrentStateValue]!.ShouldTransitionFromTrigger(trigger, out nextState))
        {
            transitioned = Transition(this.CurrentStateValue, nextState, trigger, blackboard);
        }

        nextState = transitioned ? nextState : default;
        return transitioned;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="blackboard">Shared data associated with the update.</param>
    /// <returns>True if a transition occurred; otherwise, false.</returns>
    public bool Update(TBlackboard blackboard)
    {
        if (this.states[this.CurrentStateValue]!.ShouldTransitionFromUpdate(blackboard, out TState? nextState))
        {
            var transitioned = Transition(this.CurrentStateValue, nextState, default, blackboard);
            return transitioned;
        }

        this.states[this.CurrentStateValue]!.Events?.OnUpdate.Trigger(this,
            new FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>(this.CurrentStateValue,
                this.CurrentStateValue,
                default, blackboard));

        return false;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="blackboard">Shared data associated with the update.</param>
    /// <returns>True if a transition occurred; otherwise, false.</returns>
    public bool Update(out TState startingState, TBlackboard blackboard, out TState? nextState)
    {
        startingState = this.CurrentStateValue;
        var transitioned = Update(blackboard);
        nextState = transitioned ? this.CurrentStateValue : default;
        return transitioned;
    }

    /// <summary>
    ///     Updates the finite state machine based on the current blackboard data.
    /// </summary>
    /// <param name="blackboard">Shared data associated with the update.</param>
    /// <param name="startingState">The state of the State Machine going into this method.</param>
    /// <param name="nextState">The state of the State Machine coming out of this method.</param>
    /// <param name="transitioned">Outputs True if a transition occurred, False otherwise.</param>
    public void Update(TBlackboard blackboard, out TState startingState, out TState? nextState, out bool transitioned)
    {
        startingState = this.CurrentStateValue;
        transitioned = Update(blackboard);
        nextState = transitioned ? this.CurrentStateValue : default;
    }

    /// <summary>
    ///     Updates a transition from the specified source state to the specified target state using the provided trigger.
    /// </summary>
    /// <param name="from">The source state from which the transition originates.</param>
    /// <param name="trigger">The trigger object used to initiate the transition.</param>
    /// <param name="newTo">The target state to which the transition is directed.</param>
    /// <returns>The current instance of the finite state machine after updating the transition.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> UpdateTransition(TState from, TTrigger trigger,
        TState newTo)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(newTo))
        {
            this.states[from]?.UpdateTransition(trigger, newTo);
        }

        return this;
    }

    /// <summary>
    ///     Updates a transition from the specified state to a new state using the provided trigger expression.
    /// </summary>
    /// <param name="from">The source state of the transition.</param>
    /// <param name="triggerLambda">An expression representing the trigger logic.</param>
    /// <param name="newTo">The target state to which the transition should be updated.</param>
    /// <returns>The modified finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> UpdateTransition(TState from,
        Expression<Func<TBlackboard, bool>> triggerLambda,
        TState newTo)
    {
        if (this.states.ContainsKey(from) && this.states.ContainsKey(newTo))
        {
            this.states[from]?.UpdateTransition(triggerLambda, newTo);
        }

        return this;
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
    /// <param name="hasAllStates">Outputs whether all states are present (True) or not (False).</param>
    /// <returns>The current finite state machine instance.</returns>
    public FiniteStateMachine<TState, TTrigger, TBlackboard> ValidateContainsAllStates(out bool hasAllStates)
    {
        hasAllStates = ValidateContainsAllStates();
        return this;
    }

    /// <summary>
    ///     Validates the finite state machine to ensure all states are present and their transitions are valid.
    /// </summary>
    /// <returns>
    ///     True if the finite state machine is valid; otherwise, false.
    /// </returns>
    public bool ValidateFiniteStateMachine()
    {
        if (!ValidateContainsAllStates())
        {
            return false;
        }

        foreach (KeyValuePair<TState, State<TState, TTrigger, TBlackboard>?> state in this.states)
        {
            if (state.Value == null)
            {
                return false;
            }

            if (!state.Value.ValidateTransitions())
            {
                return false;
            }
        }

        return true;
    }
}
