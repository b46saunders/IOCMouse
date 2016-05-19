using System;

namespace IOC
{
    public class Clock : IClock
    {
        public ITime GetTime()
        {
            return new TimeInstance(DateTime.Now.ToLongDateString());
        }
    }
}