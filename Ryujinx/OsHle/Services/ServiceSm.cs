using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        private const int SmNotInitialized = 0x415;

        public static long SmInitialize(ServiceCtx Context)
        {
            Context.Session.Initialize();

            return 0;
        }

        public static long SmGetService(ServiceCtx Context)
        {
            //Only for kernel version > 3.0.0.
            if (!Context.Session.IsInitialized)
            {
                //return SmNotInitialized;
            }

            string Name = string.Empty;

            for (int Index = 0; Index < 8 &&
                Context.RequestData.BaseStream.Position <
                Context.RequestData.BaseStream.Length; Index++)
            {
                byte Chr = Context.RequestData.ReadByte();

                if (Chr >= 0x20 && Chr < 0x7f)
                {
                    Name += (char)Chr;
                }
            }

            HSession Session = new HSession(Name);

            int Handle = Context.Ns.Os.Handles.GenerateId(Session);

            Context.Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return 0;
        }
    }
}