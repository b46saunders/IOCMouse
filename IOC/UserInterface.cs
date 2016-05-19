namespace IOC
{
    public class UserInterface
    {
        private readonly IDataProvider _dataProvider;
        public UserInterface(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void DisplayTime()
        {
            _dataProvider.GetNumbers();
        }
    }
}
