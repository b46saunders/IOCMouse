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
            var clock = new Clock();
            Injector.Bind<ILogger, Logger>().InSingletonScope();
            //Injector.Bind<IClock, Clock>();
            Injector.Bind<IDataProvider, DataProvider>().InSingletonScope();
            Injector.Bind<IConsumer, SomeConsumer>().InSingletonScope();
            Injector.Bind<INonInterfaceConstructor, ClassWithANonInterfaceConstructor>()
                .WithConstructorArguments("someNumber", 10);
            Injector.Bind<IClock,Clock>(clock);
            var consumerConsumer = new ConsumerConsumer(Injector.Resolve<IConsumer>());
            var test2 = Injector.Resolve<INonInterfaceConstructor>();
            
            

            //Injector.Bind<IClock, Clock>();

        }

        
    }
}
