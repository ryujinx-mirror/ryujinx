using ChocolArm64.Memory;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Ipc;
using System.IO;

using static Ryujinx.OsHle.Objects.Android.Parcel;
using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects
{
    class ViIApplicationDisplayService
    {
        public static long GetRelayService(ServiceCtx Context)
        {
            MakeObject(Context, new ViIHOSBinderDriver());

            return 0;
        }

        public static long GetSystemDisplayService(ServiceCtx Context)
        {
            MakeObject(Context, new ViISystemDisplayService());

            return 0;
        }

        public static long GetManagerDisplayService(ServiceCtx Context)
        {
            MakeObject(Context, new ViIManagerDisplayService());

            return 0;
        }

        public static long GetIndirectDisplayTransactionService(ServiceCtx Context)
        {
            MakeObject(Context, new ViIHOSBinderDriver());

            return 0;
        }

        public static long OpenDisplay(ServiceCtx Context)
        {
            string Name = GetDisplayName(Context);

            long DisplayId = Context.Ns.Os.Displays.GenerateId(new Display(Name));

            Context.ResponseData.Write(DisplayId);

            return 0;
        }

        public static long OpenLayer(ServiceCtx Context)
        {
            long LayerId = Context.RequestData.ReadInt64();
            long UserId  = Context.RequestData.ReadInt64();

            long ParcelPtr = Context.Request.ReceiveBuff[0].Position;

            byte[] Parcel = MakeIGraphicsBufferProducer(ParcelPtr);

            AMemoryHelper.WriteBytes(Context.Memory, ParcelPtr, Parcel);

            Context.ResponseData.Write((long)Parcel.Length);

            return 0;
        }

        public static long CreateStrayLayer(ServiceCtx Context)
        {
            long LayerFlags = Context.RequestData.ReadInt64();
            long DisplayId  = Context.RequestData.ReadInt64();

            long ParcelPtr = Context.Request.ReceiveBuff[0].Position;

            Display Disp = Context.Ns.Os.Displays.GetData<Display>((int)DisplayId);

            byte[] Parcel = MakeIGraphicsBufferProducer(ParcelPtr);

            AMemoryHelper.WriteBytes(Context.Memory, ParcelPtr, Parcel);

            Context.ResponseData.Write(0L);
            Context.ResponseData.Write((long)Parcel.Length);

            return 0;
        }

        public static long SetLayerScalingMode(ServiceCtx Context)
        {
            int  ScalingMode = Context.RequestData.ReadInt32();
            long Unknown     = Context.RequestData.ReadInt64();

            return 0;
        }

        public static long GetDisplayVSyncEvent(ServiceCtx Context)
        {
            string Name = GetDisplayName(Context);

            int Handle = Context.Ns.Os.Handles.GenerateId(new HEvent());

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        private static byte[] MakeIGraphicsBufferProducer(long BasePtr)
        {
            long Id        = 0x20;
            long CookiePtr = 0L;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                //flat_binder_object (size is 0x28)
                Writer.Write(2); //Type (BINDER_TYPE_WEAK_BINDER)
                Writer.Write(0); //Flags
                Writer.Write((int)(Id >> 0));
                Writer.Write((int)(Id >> 32));
                Writer.Write((int)(CookiePtr >> 0));
                Writer.Write((int)(CookiePtr >> 32));
                Writer.Write((byte)'d');
                Writer.Write((byte)'i');
                Writer.Write((byte)'s');
                Writer.Write((byte)'p');
                Writer.Write((byte)'d');
                Writer.Write((byte)'r');
                Writer.Write((byte)'v');
                Writer.Write((byte)'\0');
                Writer.Write(0L); //Pad

                return MakeParcel(MS.ToArray(), new byte[] { 0, 0, 0, 0 });
            }
        }

        private static string GetDisplayName(ServiceCtx Context)
        {
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

            return Name;
        }
    }
}