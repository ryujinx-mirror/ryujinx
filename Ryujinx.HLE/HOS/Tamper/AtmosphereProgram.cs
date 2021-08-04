using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper
{
    class AtmosphereProgram : ITamperProgram
    {
        private Parameter<long> _pressedKeys;
        private IOperation _entryPoint;

        public string Name { get; }
        public bool TampersCodeMemory { get; set; } = false;
        public ITamperedProcess Process { get; }

        public AtmosphereProgram(string name, ITamperedProcess process, Parameter<long> pressedKeys, IOperation entryPoint)
        {
            Name = name;
            Process = process;
            _pressedKeys = pressedKeys;
            _entryPoint = entryPoint;
        }

        public void Execute(ControllerKeys pressedKeys)
        {
            _pressedKeys.Value = (long)pressedKeys;
            _entryPoint.Execute();
        }
    }
}
