using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Velentr.FiniteStateMachine
{
    /// <summary>
    ///     A finite state machine.
    /// </summary>
    ///
    /// <typeparam name="TState">   Type of the state. </typeparam>
    /// <typeparam name="TTrigger"> Type of the trigger. </typeparam>
    [DebuggerDisplay("CurrentState = {CurrentStateValue}, Configurable = {CanConfigure}")]
    public class FiniteStateMachine<TState, TTrigger>
    {
        /// <summary>
        ///     The configured.
        /// </summary>
        private Guard _configured;

        /// <summary>
        ///     The current state.
        /// </summary>
        private TState _currentState;

        /// <summary>
        ///     State of the starting.
        /// </summary>
        private TState _startingState;

        /// <summary>
        ///     The states.
        /// </summary>
        private Dictionary<TState, FiniteState<TState, TTrigger>> _states;

        /// <summary>
        ///     Constructor.
        /// </summary>
        ///
        /// <param name="startingState"> State of the starting. </param>
        /// <param name="time">          (Optional) The time. </param>
        public FiniteStateMachine(TState startingState, DateTime? time = null)
        {
            _configured = new Guard();
            _startingState = startingState;
            _currentState = startingState;
            CurrentTime = time ?? DateTime.Now;

            var possibleStates = Enum.GetValues(typeof(TState));
            _states = new Dictionary<TState, FiniteState<TState, TTrigger>>(possibleStates.Length);
            for (var i = 0; i < possibleStates.Length; i++)
            {
                var value = (TState)possibleStates.GetValue(i);
                _states.Add(value, new FiniteState<TState, TTrigger>(this, value));
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we can configure.
        /// </summary>
        ///
        /// <value>
        ///     True if we can configure, false if not.
        /// </value>
        public bool CanConfigure => !_configured.Check;

        /// <summary>
        ///     Gets the current state.
        /// </summary>
        ///
        /// <value>
        ///     The current state.
        /// </value>
        public FiniteState<TState, TTrigger> CurrentState => _states[_currentState];

        /// <summary>
        ///     Gets the current state value.
        /// </summary>
        ///
        /// <value>
        ///     The current state value.
        /// </value>
        public TState CurrentStateValue => _currentState;

        /// <summary>
        ///     Gets or sets the current time.
        /// </summary>
        ///
        /// <value>
        ///     The current time.
        /// </value>
        public DateTime CurrentTime { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether this object is transitioning.
        /// </summary>
        ///
        /// <value>
        ///     True if this object is transitioning, false if not.
        /// </value>
        public bool IsTransitioning => CurrentState.IsTransitioning;

        /// <summary>
        ///     Gets the state of the starting.
        /// </summary>
        ///
        /// <value>
        ///     The starting state.
        /// </value>
        public FiniteState<TState, TTrigger> StartingState => _states[_startingState];

        /// <summary>
        ///     Gets the starting state value.
        /// </summary>
        ///
        /// <value>
        ///     The starting state value.
        /// </value>
        public TState StartingStateValue => _startingState;

        /// <summary>
        ///     Adds a transition.
        /// </summary>
        ///
        /// <param name="from">    Source for the. </param>
        /// <param name="trigger"> The trigger. </param>
        /// <param name="to">      to. </param>
        public void AddTransition(TState from, TTrigger trigger, TState to)
        {
            _states[from].AddTransition(trigger, to);
        }

        /// <summary>
        ///     Adds a transition.
        /// </summary>
        ///
        /// <param name="from">   Source for the. </param>
        /// <param name="maxAge"> The maximum age. </param>
        /// <param name="to">     to. </param>
        public void AddTransition(TState from, TimeSpan maxAge, TState to)
        {
            _states[from].AddTransition(maxAge, to);
        }

        /// <summary>
        ///     Finalize state machine.
        /// </summary>
        public void FinalizeStateMachine()
        {
            _ = _configured.CheckSet;
        }

        /// <summary>
        ///     Resets the given startingState.
        /// </summary>
        public void Reset()
        {
            Reset(_startingState);
        }

        /// <summary>
        ///     Resets the given startingState.
        /// </summary>
        ///
        /// <param name="startingState"> State of the starting. </param>
        public void Reset(TState startingState)
        {
            _startingState = startingState;
            _currentState = startingState;
            CurrentState.ResetState();
        }

        /// <summary>
        ///     Resets the and mark configurable described by startingState.
        /// </summary>
        public void ResetAndMarkConfigurable()
        {
            ResetAndMarkConfigurable(_startingState);
        }

        /// <summary>
        ///     Resets the and mark configurable described by startingState.
        /// </summary>
        ///
        /// <param name="startingState"> State of the starting. </param>
        public void ResetAndMarkConfigurable(TState startingState)
        {
            _startingState = startingState;
            _currentState = startingState;
            _configured.Reset();
            CurrentState.ResetState();
        }

        /// <summary>
        ///     Triggers the given trigger.
        /// </summary>
        ///
        /// <param name="trigger"> The trigger. </param>
        public void Trigger(TTrigger trigger)
        {
            CurrentState.Trigger(trigger);
        }

        /// <summary>
        ///     Updates the given currentTime.
        /// </summary>
        ///
        /// <param name="currentTime"> The current time. </param>
        public void Update(DateTime currentTime)
        {
            CurrentTime = currentTime;

            if (!CanConfigure)
            {
                _states[_currentState].Update();
            }
        }

        /// <summary>
        ///     Updates the state described by newState.
        /// </summary>
        ///
        /// <param name="newState"> State of the new. </param>
        internal void UpdateState(TState newState)
        {
            CurrentState.ResetState();
            _currentState = newState;
            CurrentState.ResetState();
        }
    }
}