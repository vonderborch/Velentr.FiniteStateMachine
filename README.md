# Velentr.FiniteStateMachine
A simple library containing code for a Finite State Machine definition

# Installation
A nuget package is available: [Velentr.FiniteStateMachine](https://www.nuget.org/packages/Velentr.FiniteStateMachine/)

# Usage
Step 1: Define some possible **States** (the different distinct states that the **Finite State Machine** can be in) and some **Triggers** (the possible actions that can occur on each state). Enums are easy to use for this:
```
private enum DoorStates { Opened, Closed }

private enum DoorTriggers { Open, Close }
```

Step 2: Create a FSM
```
var fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed);
```

Step 2: Define some Transitions (these are what defines what happens when a trigger is done for a particular state) and finalize the FSM (the FSM must be finalized to be used)
```
// AddTransition(FromState, Trigger, NextState)
_fsm.AddTransition(DoorStates.Closed, DoorTriggers.Open, DoorStates.Opened);
_fsm.AddTransition(DoorStates.Opened, DoorTriggers.Close, DoorStates.Closed);

_fsm.FinalizeStateMachine();
```

Step 3: Profit!
```
Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!
fsm.Trigger(DoorTriggers.Open);
Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Opened!
fsm.Trigger(DoorTriggers.Close);
Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!
```

Additional examples can be seen in the Unit Tests: https://github.com/vonderborch/Velentr.FiniteStateMachine/tree/main/Velentr.FiniteStateMachine.Test

# Transition Types
There are two types of transitions supported:
- 1: Trigger-based. These transitions are the basic ones like those defined above. When the trigger object is called (using the `.Trigger()` method), the state changes if a transition is defined.
- 2: Age-based. These transitions are based on how long the FSM has been in the current state. These rely on the `.Update()` method for the FSM being called.

The transitions can be mixed, as seen in the full example below, although only a single Age transition can be defined for each state.

# Other notes
Transitions themselves can be modified to have the state remain in the transition phase for some time, although this functionality is still to-be-tested (see: https://github.com/vonderborch/Velentr.FiniteStateMachine/issues/1)

# Full Example
```
using System;

namespace Velentr.FiniteStateMachine.DevEnv
{
    class Program
    {
        private enum DoorStates { Opened, Closed, Halfway, }

        private enum DoorTriggers { Open, Close, }

        private readonly DateTime _startingTime = DateTime.Parse("2021-09-29T07:22:16.0000000Z");

        private readonly DateTime _firstUpdate = DateTime.Parse("2021-09-29T07:22:18.0000000Z");

        private readonly DateTime _secondUpdate = DateTime.Parse("2021-09-29T07:22:20.0000000Z");

        private readonly DateTime _thirdUpdate = DateTime.Parse("2021-09-29T07:22:22.0000000Z");

        static void Main(string[] args)
        {
            var fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed);

            _fsm.AddTransition(DoorStates.Closed, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Opened, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Halfway, DoorTriggers.Close, DoorStates.Closed);
            _fsm.AddTransition(DoorStates.Halfway, DoorTriggers.Open, DoorStates.Opened);
            _fsm.AddTransition(DoorStates.Closed, DoorTriggers.Open, DoorStates.Opened);
            _fsm.AddTransition(DoorStates.Opened, DoorTriggers.Close, DoorStates.Closed);

            fsm.FinalizeStateMachine();

            _fsm.Reset(DoorStates.Opened);
            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Opened!

            _fsm.Update(_firstUpdate);
            _fsm.Trigger(DoorTriggers.Close);
            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!

            _fsm.Update(_secondUpdate);
            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Closed!

            _fsm.Update(_thirdUpdate);
            Console.WriteLine(fsm.CurrentStateValue.ToString()); // The FSM is Halfway open/closed!
        }
    }
}

```

# Future Plans
See list of issues under the Milestones: https://github.com/vonderborch/Velentr.FiniteStateMachine/milestones
