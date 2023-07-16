namespace Ryujinx.HLE.HOS.Tamper.Conditions
{
    class InputMask : ICondition
    {
        private readonly long _mask;
        private readonly Parameter<long> _input;

        public InputMask(long mask, Parameter<long> input)
        {
            _mask = mask;
            _input = input;
        }

        public bool Evaluate()
        {
            return (_input.Value & _mask) == _mask;
        }
    }
}
