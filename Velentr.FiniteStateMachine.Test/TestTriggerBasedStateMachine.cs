using System;
using NUnit.Framework;

namespace Velentr.FiniteStateMachine.Test
{
    class TestTriggerBasedStateMachine
    {

        private enum DoorStates { Opened, Closed, Halfway, }

        private enum DoorTriggers { Open, Close }

        private FiniteStateMachine<DoorStates, DoorTriggers> _fsm;

        [SetUp]
        public void Setup()
        {
            _fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed);

            _fsm.AddTransition(DoorStates.Closed, DoorTriggers.Open, DoorStates.Opened);
            _fsm.AddTransition(DoorStates.Opened, DoorTriggers.Close, DoorStates.Closed);

            _fsm.FinalizeStateMachine();
        }

        [Test]
        public void TestStartingState()
        {
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestStartingAndResetStateAreSame()
        {
            Assert.AreEqual(_fsm.StartingStateValue, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestOpeningDoor()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
            _fsm.Trigger(DoorTriggers.Open);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestClosingDoor()
        {
            _fsm.Reset(DoorStates.Opened);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);
            _fsm.Trigger(DoorTriggers.Close);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestClosingDoorTwice()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
            Assert.Throws<InvalidOperationException>(() => _fsm.Trigger(DoorTriggers.Close));
        }
    }
}
