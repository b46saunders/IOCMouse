namespace IOC.Test
{
    public class ClassWithANonInterfaceConstructor : INonInterfaceConstructor
    {
        private int _someNumber;
        public ClassWithANonInterfaceConstructor(int someNumber)
        {
            _someNumber = someNumber;
        }
    }
}