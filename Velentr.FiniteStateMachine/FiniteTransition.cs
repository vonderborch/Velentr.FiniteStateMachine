using System;
using System.Diagnostics;

namespace Velentr.FiniteStateMachine
{
    /// <summary>
    ///     A finite transition.
    /// </summary>
    ///
    /// <typeparam name="TState">   Type of the state. </typeparam>
    /// <typeparam name="TTrigger"> Type of the trigger. </typeparam>
    [DebuggerDisplay("NextState = {NextState}, TransitionType = {TransitionType}, MaxAge = {MaxAge}")]
    public class FiniteTransition<TState, TTrigger>
    {
        /// <summary>
        ///     The start transition time.
        /// </summary>
        private DateTime? _startTransitionTime;

        /// <summary>
        ///     The state.
        /// </summary>
        private FiniteState<TState, TTrigger> _state;

        /// <summary>
        ///     Constructor.
        /// </summary>
        ///
        /// <param name="state">          The state. </param>
        /// <param name="next">           The next. </param>
        /// <param name="transitionType"> The type of the transition. </param>
        public FiniteTransition(FiniteState<TState, TTrigger> state, TState next, TransitionType transitionType)
        {
            NextState = next;
            TransitionType = transitionType;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        ///
        /// <param name="state">          The state. </param>
        /// <param name="next">           The next. </param>
        /// <param name="transitionType"> The type of the transition. </param>
        /// <param name="maxAge">         The maximum age. </param>
        public FiniteTransition(FiniteState<TState, TTrigger> state, TState next, TransitionType transitionType, TimeSpan maxAge)
        {
            NextState = next;
            TransitionType = transitionType;
            MaxAge = maxAge;
        }

        /// <summary>
        ///     Gets or sets the maximum age.
        /// </summary>
        ///
        /// <value>
        ///     The maximum age.
        /// </value>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        ///     Gets or sets the state of the next.
        /// </summary>
        ///
        /// <value>
        ///     The next state.
        /// </value>
        public TState NextState { get; set; }

        /// <summary>
        ///     Gets or sets the type of the transition.
        /// </summary>
        ///
        /// <value>
        ///     The type of the transition.
        /// </value>
        public TransitionType TransitionType { get; internal set; }

        /// <summary>
        ///     Resets this object.
        /// </summary>
        public void Reset()
        {
            _startTransitionTime = null;
        }

        /// <summary>
        ///     Updates this object.
        /// </summary>
        public void Update()
        {
            if (MaxAge != null)
            {
                if (_state._stateMachine.CurrentTime - _startTransitionTime >= MaxAge)
                {
                    _state._stateMachine.UpdateState(NextState);
                }
            }
        }

        /// <summary>
        ///     Is triggered.
        /// </summary>
        internal void IsTriggered()
        {
            _startTransitionTime = _state._stateMachine.CurrentTime;
        }
    }
}