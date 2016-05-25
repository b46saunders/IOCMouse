using System;
using System.Diagnostics;
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

        [TestMethod]
        public void Resolve_InstanceLoop_InstanceUsed()
        {
            
            var moqClock = new Mock<IClock>();
            var testTime = "sometesttime...";
            moqClock.Setup(a => a.GetTime()).Returns(new TimeInstance(testTime));
            Injector.Bind<IClock, Clock>(moqClock.Object);
            

            //loop
            var stopwatch = Stopwatch.StartNew();
            var count = 1000000;
            for (int i = 0; i < count; i++)
            {
                var clock = Injector.Resolve<IClock>();
                clock.GetTime();
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            moqClock.Verify(a => a.GetTime(), Times.Exactly(count));

        }


    }
}
