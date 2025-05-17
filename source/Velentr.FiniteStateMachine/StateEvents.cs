using System.Text.Json.Serialization;
using Velentr.Core.Eventing;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Provides events for handling state transitions in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type of states in the finite state machine.</typeparam>
public class StateEvents<TState>
{
    /// <summary>
    ///     Event triggered when the state is entered.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState>> OnEnter = new();

    /// <summary>
    ///     Event triggered when the state is exited.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState>> OnExit = new();

    /// <summary>
    ///     Event triggered when the state is updated.
    /// </summary>
    [JsonIgnore] public Event<FiniteStateMachineEventArgs<TState>> OnUpdate = new();
}
