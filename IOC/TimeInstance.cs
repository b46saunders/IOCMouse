namespace IOC
{
    public class TimeInstance : ITime
    {
        public TimeInstance(string time)
        {
            Time = time;
        }
        public string Time { get; }
    }
}