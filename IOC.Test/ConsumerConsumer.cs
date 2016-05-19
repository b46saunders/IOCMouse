namespace IOC.Test
{
    public class ConsumerConsumer
    {
        private IConsumer _consumer;

        public ConsumerConsumer(IConsumer consumer)
        {
            _consumer = consumer;
        }
    }
}