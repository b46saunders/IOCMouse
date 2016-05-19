using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IOC.Test
{
    [TestClass]
    public class DataProviderTest
    {
        [TestMethod]
        public void ResolveInjection()
        {
            Injector.Bind<IClock, Clock>();
            Injector.Bind<ILogger, Logger>().InSingletonScope();
            Injector.Bind<IDataProvider, DataProvider>().InSingletonScope();
            Injector.Bind<IConsumer, SomeConsumer>().InSingletonScope();
            Injector.Bind<INonInterfaceConstructor, ClassWithANonInterfaceConstructor>();

            var c = new ConsumerConsumer(Injector.Resolve<IConsumer>());

            //Injector.Bind<IClock, Clock>();

        }

        
    }
}
