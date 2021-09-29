using System.Diagnostics;
using System.Threading;

namespace Velentr.FiniteStateMachine
{
    /// <summary>
    /// A thread-safe boolean guard/flag.
    /// </summary>
    [DebuggerDisplay("Current State = {state}")]
    public class Guard
    {
        /// <summary>
        /// The value for false
        /// </summary>
        private const int FALSE = 0;

        /// <summary>
        /// The value for true
        /// </summary>
        private const int TRUE = 1;

        /// <summary>
        /// The current state
        /// </summary>
        private int state = FALSE;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Guard"/> is check.
        /// </summary>
        /// <value><c>true</c> if check; otherwise, <c>false</c>.</value>
        public bool Check
        {
            get { return state == TRUE; }
        }

        /// <summary>
        /// Gets a value indicating whether [check set].
        /// </summary>
        /// <value><c>true</c> if [check set]; otherwise, <c>false</c>.</value>
        public bool CheckSet
        {
            get { return Interlocked.Exchange(ref state, TRUE) == FALSE; }
        }

        /// <summary>
        /// Mark the guard as being checked.
        /// </summary>
        public void MarkChecked()
        {
            Interlocked.Exchange(ref state, TRUE);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref state, FALSE);
        }
    }
}