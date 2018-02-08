using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;

namespace Ryujinx.OsHle.Objects
{
    class AudIAudioRenderer
    {
        public static long RequestUpdateAudioRenderer(ServiceCtx Context)
        {
            //buffer < unknown, 5, 0 >) -> (buffer < unknown, 6, 0 >, buffer < unknown, 6, 0 >

            long Position = Context.Request.ReceiveBuff[0].Position;

            //0x40 bytes header
            Context.Memory.WriteInt32(Position + 0x4, 0xb0); //Behavior Out State Size? (note: this is the last section)
            Context.Memory.WriteInt32(Position + 0x8, 0x18e0); //Memory Pool Out State Size?
            Context.Memory.WriteInt32(Position + 0xc, 0x600); //Voice Out State Size?
            Context.Memory.WriteInt32(Position + 0x14, 0xe0); //Effect Out State Size?
            Context.Memory.WriteInt32(Position + 0x1c, 0x20); //Sink Out State Size?
            Context.Memory.WriteInt32(Position + 0x20, 0x10); //Performance Out State Size?
            Context.Memory.WriteInt32(Position + 0x3c, 0x20e0); //Total Size (including 0x40 bytes header)

            for (int Offset = 0x40; Offset < 0x40 + 0x18e0; Offset += 0x10)
            {
                Context.Memory.WriteInt32(Position + Offset, 5);
            }

            return 0;
        }

        public static long StartAudioRenderer(ServiceCtx Context)
        {
            return 0;
        }

        public static long StopAudioRenderer(ServiceCtx Context)
        {
            return 0;
        }

        public static long QuerySystemEvent(ServiceCtx Context)
        {
            int Handle = Context.Ns.Os.Handles.GenerateId(new HEvent());

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}