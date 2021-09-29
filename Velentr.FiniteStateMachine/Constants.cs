namespace Velentr.FiniteStateMachine
{
    /// <summary>
    ///     A constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     The transition already defined exception.
        /// </summary>
        public static string TransitionAlreadyDefinedException = "A transition has already been defined for state [{0}] with trigger [{1}]!";

        /// <summary>
        ///     The transition does not exist exception.
        /// </summary>
        public static string TransitionDoesNotExistException = "A transition with the trigger [{0}] does not exist for the state [{1}]!";

        /// <summary>
        ///     The unconfigurable exception.
        /// </summary>
        public static string UnconfigurableException = "The Finite State Machine is already finalized!";

        /// <summary>
        ///     The unfinalized exception.
        /// </summary>
        public static string UnfinalizedException = "The Finite State Machine is not finalized!";
    }
}