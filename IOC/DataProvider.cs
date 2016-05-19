using System.Collections.Generic;

namespace IOC
{

    public class DataProvider : IDataProvider
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public DataProvider( IClock clock, ILogger logger)
        {
            _clock = clock;
            _logger = logger;
        }

        public IEnumerable<int> GetNumbers()
        {
            _logger.Log("GetData start");
            var time = _clock.GetTime();
            _logger.Log($"Time: {time}");
            _logger.Log("GetData end");
            return new List<int>() {time.Time.Length};
        }
    }
}
