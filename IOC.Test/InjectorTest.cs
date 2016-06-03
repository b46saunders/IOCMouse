using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Stopwatch bootWatch = Stopwatch.StartNew();
            var moqClock = new Mock<IClock>();
            var testTime = "sometesttime...";
            moqClock.Setup(a => a.GetTime()).Returns(new TimeInstance(testTime));
            Injector.Bind<IClock, Clock>(moqClock.Object);
            bootWatch.Stop();
            Console.WriteLine($"Injector boot time: {bootWatch.ElapsedMilliseconds}ms");

            //loop
            var count = 1;
            var resolveTime = new List<long>(count);
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                var clock = Injector.Resolve<IClock>();
                clock.GetTime();
                resolveTime.Add(stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();
            }
            stopwatch.Stop();
            Console.WriteLine($"Average resolve time: {resolveTime.Average()}ms");
            Console.WriteLine($"Total resolve time: {resolveTime.Sum()}ms");
            moqClock.Verify(a => a.GetTime(), Times.Exactly(count));

        }


    }
}
