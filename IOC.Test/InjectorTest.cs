using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IOC.Test
{
    [TestClass]
    public class InjectorTest
    {
        [TestMethod]
        public void Bind_NewBinding_Successful()
        {
            Injector.Bind<IConsumer, SomeConsumer>();

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Bind_DuplicateBinding_Exception()
        {
            Injector.Bind<IConsumer, SomeConsumer>();
            Injector.Bind<IConsumer, SomeConsumer>();
        }

        [TestMethod]
        public void Resolve_Instance_InstanceUsed()
        {
            var moqClock = new Mock<IClock>();
            var testTime = "sometesttime...";
            moqClock.Setup(a => a.GetTime()).Returns(new TimeInstance(testTime));
            Injector.Bind<IClock, Clock>(moqClock.Object);
            var clock = Injector.Resolve<IClock>();
            
            var time = clock.GetTime();
            moqClock.Verify(a => a.GetTime(), Times.Once);
            Assert.AreEqual(testTime,time.Time);
        }


    }
}
