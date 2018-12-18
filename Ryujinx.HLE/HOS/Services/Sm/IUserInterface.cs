using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Sm
{
    class IUserInterface : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private bool _isInitialized;

        public IUserInterface()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Initialize },
                { 1, GetService }
            };
        }

        private const int SmNotInitialized = 0x415;

        public long Initialize(ServiceCtx context)
        {
            _isInitialized = true;

            return 0;
        }

        public long GetService(ServiceCtx context)
        {
            //Only for kernel version > 3.0.0.
            if (!_isInitialized)
            {
                //return SmNotInitialized;
            }

            string name = string.Empty;

            for (int index = 0; index < 8 &&
                context.RequestData.BaseStream.Position <
                context.RequestData.BaseStream.Length; index++)
            {
                byte chr = context.RequestData.ReadByte();

                if (chr >= 0x20 && chr < 0x7f)
                {
                    name += (char)chr;
                }
            }

            if (name == string.Empty)
            {
                return 0;
            }

            KSession session = new KSession(ServiceFactory.MakeService(context.Device.System, name), name);

            if (context.Process.HandleTable.GenerateHandle(session, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(handle);

            return 0;
        }
    }
}