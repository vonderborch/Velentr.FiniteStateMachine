using System;
using NUnit.Framework;

namespace Velentr.FiniteStateMachine.Test
{
    class TestAgeBasedStateMachine
    {

        private enum DoorStates { Opened, Closed, Halfway, }

        private enum DoorTriggers { Open, Close, }

        private DateTime StartingTime = DateTime.Parse("2021-09-29T07:22:16.0000000Z");

        private DateTime FirstUpdate = DateTime.Parse("2021-09-29T07:22:18.0000000Z");

        private DateTime SecondUpdate = DateTime.Parse("2021-09-29T07:22:20.0000000Z");

        private DateTime ThirdUpdate = DateTime.Parse("2021-09-29T07:22:22.0000000Z");

        private FiniteStateMachine<DoorStates, DoorTriggers> _fsm;

        [SetUp]
        public void Setup()
        {
            _fsm = new FiniteStateMachine<DoorStates, DoorTriggers>(DoorStates.Closed, StartingTime);

            _fsm.AddTransition(DoorStates.Closed, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Halfway, new TimeSpan(0, 0, 1), DoorStates.Opened);
            _fsm.AddTransition(DoorStates.Opened, new TimeSpan(0, 0, 2), DoorStates.Halfway);
            _fsm.AddTransition(DoorStates.Halfway, new TimeSpan(0, 0, 1), DoorStates.Closed);

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
        public void TestNoTriggers()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
            Assert.Throws<InvalidOperationException>(() => _fsm.Trigger(DoorTriggers.Open));
        }

        [Test]
        public void Test()
        {
            _fsm.Reset(DoorStates.Closed);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(FirstUpdate);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);

            _fsm.Update(SecondUpdate);
            Assert.AreEqual(DoorStates.Halfway, _fsm.CurrentStateValue);

            _fsm.Update(ThirdUpdate);
            Assert.AreEqual(DoorStates.Closed, _fsm.CurrentStateValue);
        }
    }
}
