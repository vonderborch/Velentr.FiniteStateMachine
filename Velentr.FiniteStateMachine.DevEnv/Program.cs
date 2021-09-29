using System;

namespace Velentr.FiniteStateMachine.DevEnv
{
    class Program
    {
        private enum DoorStates { Opened, Closed }

        private enum DoorTriggers { Open, Close }

        static void Main(string[] args)
        {
            var fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed);

            fsm.AddTransition(DoorStates.Closed, DoorTriggers.Open, DoorStates.Opened);
            fsm.AddTransition(DoorStates.Opened, DoorTriggers.Close, DoorStates.Closed);

            fsm.FinalizeStateMachine();

            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!
            fsm.Trigger(DoorTriggers.Open);

            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Opened!
            fsm.Trigger(DoorTriggers.Close);

            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!
        }
    }
}
