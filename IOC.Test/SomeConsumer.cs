namespace IOC.Test
{
    public class SomeConsumer : IConsumer
    {
        private IClock _clock;
        private ILogger _logger;
        private IDataProvider _dataProvider;

        public SomeConsumer(IClock clock, ILogger logger, IDataProvider dataProvider)
        {
            _clock = clock;
            _logger = logger;
            _dataProvider = dataProvider;
        }
    }
}