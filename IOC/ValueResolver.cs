using System.ComponentModel;

namespace IOC
{
    public class ValueResolver<T> : IObjectResolver where T: struct
    {
        private readonly T _value;
        public ValueResolver(T value)
        {
            _value = value;
        }

        public object Resolve()
        {
            return _value;
        }
    }
}