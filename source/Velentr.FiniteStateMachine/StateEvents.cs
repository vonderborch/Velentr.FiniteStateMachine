using System.Text.Json.Serialization;
using Velentr.Core.Eventing;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Provides events for handling state transitions in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type of states in the finite state machine.</typeparam>
/// <typeparam name="TTrigger">The type of triggers in the finite state machine.</typeparam>
/// <typeparam name="TBlackboard">The type of blackboard data associated with the state.</typeparam>
public class StateEvents<TState, TTrigger, TBlackboard>
    where TState : notnull
    where TTrigger : IEquatable<TTrigger>
    where TBlackboard : class
{
    /// <summary>
    ///     Event triggered when the state is entered.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> OnEnter = new();

    /// <summary>
    ///     Event triggered when the state is exited.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> OnExit = new();

    /// <summary>
    ///     Event triggered when the state is updated.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState, TTrigger, TBlackboard>> OnUpdate = new();
}
