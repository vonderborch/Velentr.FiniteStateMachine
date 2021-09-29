using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Velentr.FiniteStateMachine
{
    /// <summary>
    ///     A finite state.
    /// </summary>
    ///
    /// <typeparam name="TState">   Type of the state. </typeparam>
    /// <typeparam name="TTrigger"> Type of the trigger. </typeparam>
    [DebuggerDisplay("State = {Value}, IsTransitioning = {IsTransitioning}, TransitionCount = {_transitions.Count}, MaxAge = {_maxAge}")]
    public class FiniteState<TState, TTrigger>
    {
        /// <summary>
        ///     The state machine.
        /// </summary>
        internal FiniteStateMachine<TState, TTrigger> _stateMachine;

        /// <summary>
        ///     The age transition.
        /// </summary>
        private FiniteTransition<TState, TTrigger> _ageTransition;

        /// <summary>
        ///     The current transition.
        /// </summary>
        private FiniteTransition<TState, TTrigger> _currentTransition;

        /// <summary>
        ///     The maximum age.
        /// </summary>
        private TimeSpan? _maxAge;

        /// <summary>
        ///     The start state time.
        /// </summary>
        private DateTime _startStateTime;

        /// <summary>
        ///     The transitions.
        /// </summary>
        private Dictionary<TTrigger, FiniteTransition<TState, TTrigger>> _transitions;

        /// <summary>
        ///     Constructor.
        /// </summary>
        ///
        /// <param name="stateMachine"> The state machine. </param>
        /// <param name="state">        The state. </param>
        public FiniteState(FiniteStateMachine<TState, TTrigger> stateMachine, TState state)
        {
            _stateMachine = stateMachine;
            Value = state;
            _transitions = new Dictionary<TTrigger, FiniteTransition<TState, TTrigger>>();
            _maxAge = null;
            _startStateTime = _stateMachine.CurrentTime;
        }

        /// <summary>
        ///     Gets the age transition.
        /// </summary>
        ///
        /// <value>
        ///     The age transition.
        /// </value>
        public FiniteTransition<TState, TTrigger> AgeTransition => _ageTransition;

        /// <summary>
        ///     Gets a value indicating whether this object is transitioning.
        /// </summary>
        ///
        /// <value>
        ///     True if this object is transitioning, false if not.
        /// </value>
        public bool IsTransitioning => !_stateMachine.CanConfigure && _currentTransition != null;

        /// <summary>
        ///     Gets the transitions.
        /// </summary>
        ///
        /// <value>
        ///     The transitions.
        /// </value>
        public Dictionary<TTrigger, FiniteTransition<TState, TTrigger>> Transitions => _transitions;

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        ///
        /// <value>
        ///     The value.
        /// </value>
        public TState Value { get; internal set; }

        /// <summary>
        ///     Adds a transition to 'to'.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        ///
        /// <param name="trigger"> The trigger. </param>
        /// <param name="to">      to. </param>
        public void AddTransition(TTrigger trigger, TState to)
        {
            if (!_stateMachine.CanConfigure)
            {
                throw new InvalidOperationException(Constants.UnconfigurableException);
            }

            if (_transitions.ContainsKey(trigger))
            {
                throw new InvalidOperationException(string.Format(Constants.TransitionAlreadyDefinedException, Value.ToString(), trigger.ToString()));
            }

            _transitions[trigger] = new FiniteTransition<TState, TTrigger>(this, to, TransitionType.Trigger);
        }

        /// <summary>
        ///     Adds a transition to 'to'.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        ///
        /// <param name="maxAge"> The maximum age. </param>
        /// <param name="to">     to. </param>
        public void AddTransition(TimeSpan maxAge, TState to)
        {
            if (!_stateMachine.CanConfigure)
            {
                throw new InvalidOperationException(Constants.UnconfigurableException);
            }

            _maxAge = maxAge;
            _ageTransition = new FiniteTransition<TState, TTrigger>(this, to, TransitionType.Age);
        }

        /// <summary>
        ///     Removes the age transition.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        ///
        /// <returns>
        ///     A FiniteTransition&lt;TState,TTrigger&gt;
        /// </returns>
        public FiniteTransition<TState, TTrigger> RemoveAgeTransition()
        {
            if (!_stateMachine.CanConfigure)
            {
                throw new InvalidOperationException(Constants.UnconfigurableException);
            }

            _maxAge = null;
            var value = _ageTransition;
            _ageTransition = null;
            return value;
        }

        /// <summary>
        ///     Removes the transition described by trigger.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Thrown when a value was unexpectedly null.
        /// </exception>
        ///
        /// <param name="trigger"> The trigger. </param>
        ///
        /// <returns>
        ///     A FiniteTransition&lt;TState,TTrigger&gt;
        /// </returns>
        public FiniteTransition<TState, TTrigger> RemoveTransition(TTrigger trigger)
        {
            if (!_stateMachine.CanConfigure)
            {
                throw new InvalidOperationException(Constants.UnconfigurableException);
            }

            if (_transitions.ContainsKey(trigger))
            {
                var value = _transitions[trigger];
                _transitions.Remove(trigger);
                return value;
            }
            else
            {
                throw new NullReferenceException(string.Format(Constants.TransitionDoesNotExistException, trigger.ToString(), Value.ToString()));
            }
        }

        /// <summary>
        ///     Triggers the given trigger.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Thrown when a value was unexpectedly null.
        /// </exception>
        ///
        /// <param name="trigger"> The trigger. </param>
        public void Trigger(TTrigger trigger)
        {
            if (_stateMachine.CanConfigure)
            {
                throw new InvalidOperationException(Constants.UnfinalizedException);
            }

            if (!_transitions.ContainsKey(trigger))
            {
                throw new InvalidOperationException(string.Format(Constants.TransitionDoesNotExistException, trigger.ToString(), Value.ToString()));
            }

            _currentTransition = _transitions[trigger];
            CheckIfAutoChangeState();
        }

        /// <summary>
        ///     Updates this object.
        /// </summary>
        public void Update()
        {
            if (!_stateMachine.CanConfigure)
            {
                // are we already transitioning?
                if (IsTransitioning)
                {
                    _currentTransition.Update();
                }
                // otherwise, let's check if we have been in this state long enough to move to the next
                else if (_maxAge != null)
                {
                    if (_stateMachine.CurrentTime - _startStateTime > _maxAge)
                    {
                        _currentTransition = _ageTransition;
                        CheckIfAutoChangeState();
                    }
                }
            }
        }

        /// <summary>
        ///     Resets the state.
        /// </summary>
        internal void ResetState()
        {
            _currentTransition = null;
            foreach (var transition in _transitions)
            {
                transition.Value.Reset();
            }
            _ageTransition?.Reset();
            _startStateTime = _stateMachine.CurrentTime;
        }

        /// <summary>
        ///     Check if automatic change state.
        /// </summary>
        private void CheckIfAutoChangeState()
        {
            if (_currentTransition.MaxAge == null || _stateMachine.CurrentTime - _startStateTime >= _maxAge)
            {
                _stateMachine.UpdateState(_currentTransition.NextState);
            }
        }
    }
}