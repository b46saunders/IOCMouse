namespace IOC
{
    public class ObjectResolver : IObjectResolver
    {
        private readonly object _value;
        public ObjectResolver(object value)
        {
            _value = value;
        }

        public object Resolve()
        {
            return _value;
        }
    }
}