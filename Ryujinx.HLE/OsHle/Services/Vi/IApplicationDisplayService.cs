using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.HLE.OsHle.Services.Android.Parcel;

namespace Ryujinx.HLE.OsHle.Services.Vi
{
    class IApplicationDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private IdDictionary Displays;

        public IApplicationDisplayService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 100,  GetRelayService                      },
                { 101,  GetSystemDisplayService              },
                { 102,  GetManagerDisplayService             },
                { 103,  GetIndirectDisplayTransactionService },
                { 1010, OpenDisplay                          },
                { 1020, CloseDisplay                         },
                { 1102, GetDisplayResolution                 },
                { 2020, OpenLayer                            },
                { 2021, CloseLayer                           },
                { 2030, CreateStrayLayer                     },
                { 2031, DestroyStrayLayer                    },
                { 2101, SetLayerScalingMode                  },
                { 5202, GetDisplayVSyncEvent                 }
            };

            Displays = new IdDictionary();
        }

        public long GetRelayService(ServiceCtx Context)
        {
            MakeObject(Context, new IHOSBinderDriver(Context.Ns.Gpu.Renderer));

            return 0;
        }

        public long GetSystemDisplayService(ServiceCtx Context)
        {
            MakeObject(Context, new ISystemDisplayService());

            return 0;
        }

        public long GetManagerDisplayService(ServiceCtx Context)
        {
            MakeObject(Context, new IManagerDisplayService());

            return 0;
        }

        public long GetIndirectDisplayTransactionService(ServiceCtx Context)
        {
            MakeObject(Context, new IHOSBinderDriver(Context.Ns.Gpu.Renderer));

            return 0;
        }

        public long OpenDisplay(ServiceCtx Context)
        {
            string Name = GetDisplayName(Context);

            long DisplayId = Displays.Add(new Display(Name));

            Context.ResponseData.Write(DisplayId);

            return 0;
        }

        public long CloseDisplay(ServiceCtx Context)
        {
            int DisplayId = Context.RequestData.ReadInt32();

            Displays.Delete(DisplayId);

            return 0;
        }

        public long GetDisplayResolution(ServiceCtx Context)
        {
            long DisplayId = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(1280);
            Context.ResponseData.Write(720);

            return 0;
        }

        public long OpenLayer(ServiceCtx Context)
        {
            long LayerId = Context.RequestData.ReadInt64();
            long UserId  = Context.RequestData.ReadInt64();

            long ParcelPtr = Context.Request.ReceiveBuff[0].Position;

            byte[] Parcel = MakeIGraphicsBufferProducer(ParcelPtr);

            Context.Memory.WriteBytes(ParcelPtr, Parcel);

            Context.ResponseData.Write((long)Parcel.Length);

            return 0;
        }

        public long CloseLayer(ServiceCtx Context)
        {
            long LayerId = Context.RequestData.ReadInt64();

            return 0;
        }

        public long CreateStrayLayer(ServiceCtx Context)
        {
            long LayerFlags = Context.RequestData.ReadInt64();
            long DisplayId  = Context.RequestData.ReadInt64();

            long ParcelPtr = Context.Request.ReceiveBuff[0].Position;

            Display Disp = Displays.GetData<Display>((int)DisplayId);

            byte[] Parcel = MakeIGraphicsBufferProducer(ParcelPtr);

            Context.Memory.WriteBytes(ParcelPtr, Parcel);

            Context.ResponseData.Write(0L);
            Context.ResponseData.Write((long)Parcel.Length);

            return 0;
        }

        public long DestroyStrayLayer(ServiceCtx Context)
        {
            return 0;
        }

        public long SetLayerScalingMode(ServiceCtx Context)
        {
            int  ScalingMode = Context.RequestData.ReadInt32();
            long Unknown     = Context.RequestData.ReadInt64();

            return 0;
        }

        public long GetDisplayVSyncEvent(ServiceCtx Context)
        {
            string Name = GetDisplayName(Context);

            int Handle = Context.Process.HandleTable.OpenHandle(Context.Ns.Os.VsyncEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        private byte[] MakeIGraphicsBufferProducer(long BasePtr)
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

        private string GetDisplayName(ServiceCtx Context)
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