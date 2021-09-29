using System;
using NUnit.Framework;

namespace Velentr.FiniteStateMachine.Test
{
    class TestMixedStateMachine
    {

        private enum DoorStates { Opened, Closed, Halfway, }

        private enum DoorTriggers { Open, Close, }

        private readonly DateTime _startingTime = DateTime.Parse("2021-09-29T07:22:16.0000000Z");

        private readonly DateTime _firstUpdate = DateTime.Parse("2021-09-29T07:22:18.0000000Z");

        private readonly DateTime _secondUpdate = DateTime.Parse("2021-09-29T07:22:20.0000000Z");

        private readonly DateTime _thirdUpdate = DateTime.Parse("2021-09-29T07:22:22.0000000Z");

        private FiniteStateMachine<DoorStates, DoorTriggers> _fsm;

        [SetUp]
        public void Setup()
        {
            _fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed, _startingTime);

            _fsm.AddTransition(DoorStates.Closed, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Opened, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Halfway, DoorTriggers.Close, DoorStates.Closed);
            _fsm.AddTransition(DoorStates.Halfway, DoorTriggers.Open, DoorStates.Opened);
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
            Assert.AreEqual(_fsm.CurrentStateValue, _fsm.StartingStateValue);
        }

        [Test]
        public void TestHalfwayOpeningAutomatic()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(_firstUpdate);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);

            _fsm.Update(_thirdUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestHalfwayClosingAutomatic()
        {
            _fsm.Reset(DoorStates.Opened);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_firstUpdate);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);

            _fsm.Update(_thirdUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestImmediateOpeningDoor()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
            _fsm.Update(_firstUpdate);
            _fsm.Trigger(DoorTriggers.Open);

            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_thirdUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestImmediateClosingDoor()
        {
            _fsm.Reset(DoorStates.Opened);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_firstUpdate);
            _fsm.Trigger(DoorTriggers.Close);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(_thirdUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestClosingHalfwayDoor()
        {
            _fsm.Reset(DoorStates.Opened);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_firstUpdate);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);

            _fsm.Trigger(DoorTriggers.Close);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
        }

        [Test]
        public void TestOpeningHalfwayDoor()
        {
            _fsm.Reset(DoorStates.Opened);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_firstUpdate);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);

            _fsm.Update(_secondUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);

            _fsm.Trigger(DoorTriggers.Open);
            Assert.AreEqual(DoorStates.Opened, _fsm.CurrentStateValue);
        }
    }
}
