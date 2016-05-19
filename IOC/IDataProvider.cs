using System.Collections.Generic;

namespace IOC
{
    public interface IDataProvider
    {
        IEnumerable<int> GetNumbers();
    }
}