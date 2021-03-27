using Ryujinx.HLE.HOS.Services.Hid;

namespace Ryujinx.HLE.HOS.Tamper
{
    interface ITamperProgram
    {
        ITamperedProcess Process { get; }
        void Execute(ControllerKeys pressedKeys);
    }
}
