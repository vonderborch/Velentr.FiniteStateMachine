using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Velentr.Core.Json;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Represents a concrete implementation of a state in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type of the state value.</typeparam>
/// <typeparam name="TTrigger">The type of the trigger value.</typeparam>
/// <typeparam name="TBlackboard">The type of the blackboard data context.</typeparam>
[DebuggerDisplay("State = {StateValue}")]
public class State<TState, TTrigger, TBlackboard> where TState : notnull
    where TTrigger : IEquatable<TTrigger>
    where TBlackboard : class
{
    [JsonIgnore] private readonly Dictionary<Expression<Func<TBlackboard, bool>>, Func<TBlackboard, bool>>
        compiledFunctionalTransitions;

    [JsonIgnore] private readonly Dictionary<Expression<Func<TBlackboard, bool>>, TState> functionalTransitions;

    [JsonIgnore] private Dictionary<string, TState> serializedFunctionalTransitions;

    [JsonIgnore] private Dictionary<TTrigger, TState> transitions;

    protected internal State()
    {
        this.transitions = new Dictionary<TTrigger, TState>();
        this.functionalTransitions = new Dictionary<Expression<Func<TBlackboard, bool>>, TState>();
        this.compiledFunctionalTransitions =
            new Dictionary<Expression<Func<TBlackboard, bool>>, Func<TBlackboard, bool>>();
        this.serializedFunctionalTransitions = new Dictionary<string, TState>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="State{TState, TTrigger, TBlackboard}" /> class with the specified
    ///     state value.
    /// </summary>
    /// <param name="state">The value of the state.</param>
    public State(TState state)
    {
        this.StateValue = state;
        this.transitions = new Dictionary<TTrigger, TState>();
        this.functionalTransitions = new Dictionary<Expression<Func<TBlackboard, bool>>, TState>();
        this.compiledFunctionalTransitions =
            new Dictionary<Expression<Func<TBlackboard, bool>>, Func<TBlackboard, bool>>();
        this.serializedFunctionalTransitions = new Dictionary<string, TState>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="State{TState, TTrigger, TBlackboard}" /> class with the specified
    ///     state value and
    ///     transitions.
    /// </summary>
    /// <param name="stateValue">The value of the state.</param>
    /// <param name="transitions">The transitions available from this state.</param>
    /// <param name="serializedFunctionalTransitions">The serialized functional transitions available from this state.</param>
    [JsonConstructor]
    public State(TState stateValue, Dictionary<TTrigger, TState> transitions,
        Dictionary<string, TState> serializedFunctionalTransitions)
    {
        this.StateValue = stateValue;
        this.transitions = transitions;
        this.serializedFunctionalTransitions = serializedFunctionalTransitions;
        this.Events = new StateEvents<TState, TTrigger, TBlackboard>();
        this.functionalTransitions = new Dictionary<Expression<Func<TBlackboard, bool>>, TState>();
        this.compiledFunctionalTransitions =
            new Dictionary<Expression<Func<TBlackboard, bool>>, Func<TBlackboard, bool>>();

        // Reconstruct functional transitions
        foreach (KeyValuePair<string, TState> transition in serializedFunctionalTransitions)
        {
            try
            {
                Expression<Func<TBlackboard, bool>> deserializedTrigger =
                    ExpressionSerializer<Func<TBlackboard, bool>>.DeserializeExpression(transition.Key);
                this.functionalTransitions.Add(deserializedTrigger, transition.Value);
                this.compiledFunctionalTransitions.Add(deserializedTrigger, deserializedTrigger.Compile());
            }
            catch
            {
                // Log or handle deserialization errors gracefully
                Console.WriteLine($"Failed to deserialize functional transition: {transition.Key}");
            }
        }
    }

    /// <summary>
    ///     Gets the lambda (functional) transitions available from this state.
    /// </summary>
    [JsonIgnore]
    public Dictionary<Expression<Func<TBlackboard, bool>>, TState> FunctionalTransitions =>
        new(this.functionalTransitions);

    /// <summary>
    ///     Provides access to event handlers for state transitions in a finite state machine.
    /// </summary>
    [JsonIgnore]
    public StateEvents<TState, TTrigger, TBlackboard>? Events { get; set; }

    /// <summary>
    ///     Represents a dictionary of serialized functional transitions, where keys are string representations
    ///     of lambda expressions and values are the corresponding next state values. This property is used
    ///     for serialization when saving or transmitting state machine configurations.
    /// </summary>
    [JsonPropertyName("serializedFunctionalTransitions")]
    public Dictionary<string, TState> SerializedFunctionalTransitions
    {
        get
        {
            if (this.serializedFunctionalTransitions.Count == 0)
            {
                foreach (KeyValuePair<Expression<Func<TBlackboard, bool>>, TState> transition in this
                             .functionalTransitions)
                {
                    var serializedTrigger = ExpressionSerializer<Func<TBlackboard, bool>>.SerializeExpression(transition.Key);
                    this.serializedFunctionalTransitions.Add(serializedTrigger, transition.Value);
                }
            }

            return this.serializedFunctionalTransitions;
        }
        protected internal set
        {
            this.serializedFunctionalTransitions = value;

            // Reconstruct functional transitions
            foreach (KeyValuePair<string, TState> transition in value)
            {
                try
                {
                    Expression<Func<TBlackboard, bool>> deserializedTrigger =
                        ExpressionSerializer<Func<TBlackboard, bool>>.DeserializeExpression(transition.Key);
                    this.functionalTransitions.Add(deserializedTrigger, transition.Value);
                    this.compiledFunctionalTransitions.Add(deserializedTrigger, deserializedTrigger.Compile());
                }
                catch
                {
                    // Log or handle deserialization errors gracefully
                    Console.WriteLine($"Failed to deserialize functional transition: {transition.Key}");
                }
            }
        }
    }

    /// <summary>
    ///     Gets the value of the current state.
    /// </summary>
    [JsonPropertyName("stateValue")]
    public TState StateValue { get; protected internal set; }

    /// <summary>
    ///     Gets the transitions available from this state.
    /// </summary>
    [JsonPropertyName("transitions")]
    public Dictionary<TTrigger, TState> Transitions
    {
        get => new(this.transitions);
        protected internal set => this.transitions = value;
    }

    /// <summary>
    ///     Adds a transition to another state triggered by a specific event or condition.
    /// </summary>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> AddTransition(TTrigger trigger, TState nextState)
    {
        this.transitions.TryAdd(trigger, nextState);

        return this;
    }

    /// <summary>
    ///     Adds a transition to another state triggered by a specific condition in blackboard data.
    /// </summary>
    /// <param name="triggerLambda">The trigger that causes the transition.</param>
    /// <param name="nextState">The state to transition to.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> AddTransition(Expression<Func<TBlackboard, bool>> triggerLambda,
        TState nextState)
    {
        if (this.functionalTransitions.TryAdd(triggerLambda, nextState))
        {
            this.compiledFunctionalTransitions.Add(triggerLambda, triggerLambda.Compile());
            this.serializedFunctionalTransitions.Add(
                ExpressionSerializer<Func<TBlackboard, bool>>.SerializeExpression(triggerLambda), nextState);
        }

        return this;
    }

    /// <summary>
    ///     Deserializes a JSON string into a State object.
    /// </summary>
    /// <param name="serializedState">The JSON string representing the state.</param>
    /// <param name="options">Optional serializer options.</param>
    /// <returns>The deserialized State instance, or null if serialization failed.</returns>
    public static State<TState, TTrigger, TBlackboard>? Deserialize(string serializedState,
        JsonSerializerOptions? options = null)
    {
        options ??= Constants.SerializationOptions;
        State<TState, TTrigger, TBlackboard>? state = JsonSerializer.Deserialize<State<TState, TTrigger, TBlackboard>?>(
            serializedState, options);
        return state;
    }

    /// <summary>
    ///     Registers an event handler for the Enter event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> RegisterOnEnterEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
    {
        this.Events ??= new StateEvents<TState, TTrigger, TBlackboard>();

        this.Events.OnEnter += handler;
        return this;
    }

    /// <summary>
    ///     Registers an event handler for the Exit event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> RegisterOnExitEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
    {
        this.Events ??= new StateEvents<TState, TTrigger, TBlackboard>();

        this.Events.OnExit += handler;
        return this;
    }

    /// <summary>
    ///     Registers an event handler for the Update event of the state.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> RegisterOnUpdateEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
    {
        this.Events ??= new StateEvents<TState, TTrigger, TBlackboard>();

        this.Events.OnUpdate += handler;
        return this;
    }

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="trigger">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> RemoveTransition(TTrigger trigger)
    {
        this.transitions.Remove(trigger);

        return this;
    }

    /// <summary>
    ///     Removes a transition associated with a specific trigger.
    /// </summary>
    /// <param name="triggerLambda">The trigger associated with the transition to remove.</param>
    /// <returns>The current state instance.</returns>
    public State<TState, TTrigger, TBlackboard> RemoveTransition(Expression<Func<TBlackboard, bool>> triggerLambda)
    {
        if (this.functionalTransitions.Remove(triggerLambda))
        {
            this.compiledFunctionalTransitions.Remove(triggerLambda);
            this.serializedFunctionalTransitions.Clear();
        }

        return this;
    }

    /// <summary>
    ///     Serializes the current state object into a JSON string using the specified serialization options.
    /// </summary>
    /// <param name="options">
    ///     Optional JsonSerializerOptions to control the serialization process. Defaults to
    ///     Constants.SerializationOptions.
    /// </param>
    /// <returns>A JSON string representation of the current state object.</returns>
    public string Serialize(JsonSerializerOptions? options = null)
    {
        options ??= Constants.SerializationOptions;
        var serializedState = JsonSerializer.Serialize(this, options);
        return serializedState;
    }

    /// <summary>
    ///     Determines whether a transition should occur based on the given trigger.
    /// </summary>
    /// <param name="trigger">The trigger to evaluate.</param>
    /// <param name="nextState">The next state if a transition should occur.</param>
    /// <returns>True if a transition should occur; otherwise, false.</returns>
    public bool ShouldTransitionFromTrigger(TTrigger trigger, out TState? nextState)
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
    public bool ShouldTransitionFromUpdate(TBlackboard blackboard, out TState? nextState)
    {
        if (this.functionalTransitions.Count > 0)
        {
            foreach (KeyValuePair<Expression<Func<TBlackboard, bool>>, TState> transition in this.functionalTransitions)
            {
                Func<TBlackboard, bool> compiledTrigger = this.compiledFunctionalTransitions[transition.Key];
                if (compiledTrigger(blackboard))
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
    public State<TState, TTrigger, TBlackboard> UnregisterOnEnterEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
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
    public State<TState, TTrigger, TBlackboard> UnregisterOnExitEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
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
    public State<TState, TTrigger, TBlackboard> UnregisterOnUpdateEvent(
        EventHandler<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> handler)
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
    public State<TState, TTrigger, TBlackboard> UpdateTransition(TTrigger trigger, TState newNextState)
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
    public State<TState, TTrigger, TBlackboard> UpdateTransition(Expression<Func<TBlackboard, bool>> triggerLambda,
        TState newNextState)
    {
        if (this.functionalTransitions.ContainsKey(triggerLambda))
        {
            this.functionalTransitions[triggerLambda] = newNextState;
            this.compiledFunctionalTransitions[triggerLambda] = triggerLambda.Compile();
            this.serializedFunctionalTransitions.Clear();
        }

        return this;
    }

    /// <summary>
    ///     Validates that transitions do not point to the current state.
    /// </summary>
    /// <returns>
    ///     True if all transitions are valid (do not point to the current state), otherwise false.
    /// </returns>
    public bool ValidateTransitions()
    {
        foreach (KeyValuePair<TTrigger, TState> transition in this.transitions)
        {
            if (transition.Value.Equals(this.StateValue))
            {
                return false;
            }
        }

        foreach (KeyValuePair<Expression<Func<TBlackboard, bool>>, TState> transition in this.functionalTransitions)
        {
            if (transition.Value.Equals(this.StateValue))
            {
                return false;
            }
        }

        return true;
    }
}
