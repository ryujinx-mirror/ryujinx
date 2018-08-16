using ChocolArm64.Memory;
using Ryujinx.HLE.Logging;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Nv.NvHostCtrl
{
    class NvHostCtrlIoctl
    {
        private static ConcurrentDictionary<Process, NvHostCtrlUserCtx> UserCtxs;

        private static bool IsProductionMode = true;

        static NvHostCtrlIoctl()
        {
            UserCtxs = new ConcurrentDictionary<Process, NvHostCtrlUserCtx>();

            if (Set.NxSettings.Settings.TryGetValue("nv!rmos_set_production_mode", out object ProductionModeSetting))
            {
                IsProductionMode = ((string)ProductionModeSetting) != "0"; // Default value is ""
            }
        }

        public static int ProcessIoctl(ServiceCtx Context, int Cmd)
        {
            switch (Cmd & 0xffff)
            {
                case 0x0014: return SyncptRead    (Context);
                case 0x0015: return SyncptIncr    (Context);
                case 0x0016: return SyncptWait    (Context);
                case 0x0019: return SyncptWaitEx  (Context);
                case 0x001a: return SyncptReadMax (Context);
                case 0x001b: return GetConfig     (Context);
                case 0x001d: return EventWait     (Context);
                case 0x001e: return EventWaitAsync(Context);
                case 0x001f: return EventRegister (Context);
            }

            throw new NotImplementedException(Cmd.ToString("x8"));
        }

        private static int SyncptRead(ServiceCtx Context)
        {
            return SyncptReadMinOrMax(Context, Max: false);
        }

        private static int SyncptIncr(ServiceCtx Context)
        {
            long InputPosition = Context.Request.GetBufferType0x21().Position;

            int Id = Context.Memory.ReadInt32(InputPosition);

            if ((uint)Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvResult.InvalidInput;
            }

            GetUserCtx(Context).Syncpt.Increment(Id);

            return NvResult.Success;
        }

        private static int SyncptWait(ServiceCtx Context)
        {
            return SyncptWait(Context, Extended: false);
        }

        private static int SyncptWaitEx(ServiceCtx Context)
        {
            return SyncptWait(Context, Extended: true);
        }

        private static int SyncptReadMax(ServiceCtx Context)
        {
            return SyncptReadMinOrMax(Context, Max: true);
        }

        private static int GetConfig(ServiceCtx Context)
        {
            if (!IsProductionMode)
            {
                long InputPosition  = Context.Request.GetBufferType0x21().Position;
                long OutputPosition = Context.Request.GetBufferType0x22().Position;

                string Domain = AMemoryHelper.ReadAsciiString(Context.Memory, InputPosition + 0, 0x41);
                string Name   = AMemoryHelper.ReadAsciiString(Context.Memory, InputPosition + 0x41, 0x41);

                if (Set.NxSettings.Settings.TryGetValue($"{Domain}!{Name}", out object NvSetting))
                {
                    byte[] SettingBuffer = new byte[0x101];

                    if (NvSetting is string StringValue)
                    {
                        if (StringValue.Length > 0x100)
                        {
                            Context.Device.Log.PrintError(Logging.LogClass.ServiceNv, $"{Domain}!{Name} String value size is too big!");
                        }
                        else
                        {
                            SettingBuffer = Encoding.ASCII.GetBytes(StringValue + "\0");
                        }
                    }

                    if (NvSetting is int IntValue)
                    {
                        SettingBuffer = BitConverter.GetBytes(IntValue);
                    }
                    else if (NvSetting is bool BoolValue)
                    {
                        SettingBuffer[0] = BoolValue ? (byte)1 : (byte)0;
                    }
                    else
                    {
                        throw new NotImplementedException(NvSetting.GetType().Name);
                    }

                    Context.Memory.WriteBytes(OutputPosition + 0x82, SettingBuffer);

                    Context.Device.Log.PrintDebug(Logging.LogClass.ServiceNv, $"Got setting {Domain}!{Name}");
                }

                return NvResult.Success;
            }

            return NvResult.NotAvailableInProduction;
        }

        private static int EventWait(ServiceCtx Context)
        {
            return EventWait(Context, Async: false);
        }

        private static int EventWaitAsync(ServiceCtx Context)
        {
            return EventWait(Context, Async: true);
        }

        private static int EventRegister(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            int EventId = Context.Memory.ReadInt32(InputPosition);

            Context.Device.Log.PrintStub(LogClass.ServiceNv, "Stubbed.");

            return NvResult.Success;
        }

        private static int SyncptReadMinOrMax(ServiceCtx Context, bool Max)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostCtrlSyncptRead Args = AMemoryHelper.Read<NvHostCtrlSyncptRead>(Context.Memory, InputPosition);

            if ((uint)Args.Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvResult.InvalidInput;
            }

            if (Max)
            {
                Args.Value = GetUserCtx(Context).Syncpt.GetMax(Args.Id);
            }
            else
            {
                Args.Value = GetUserCtx(Context).Syncpt.GetMin(Args.Id);
            }

            AMemoryHelper.Write(Context.Memory, OutputPosition, Args);

            return NvResult.Success;
        }

        private static int SyncptWait(ServiceCtx Context, bool Extended)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostCtrlSyncptWait Args = AMemoryHelper.Read<NvHostCtrlSyncptWait>(Context.Memory, InputPosition);

            NvHostSyncpt Syncpt = GetUserCtx(Context).Syncpt;

            if ((uint)Args.Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvResult.InvalidInput;
            }

            int Result;

            if (Syncpt.MinCompare(Args.Id, Args.Thresh))
            {
                Result = NvResult.Success;
            }
            else if (Args.Timeout == 0)
            {
                Result = NvResult.TryAgain;
            }
            else
            {
                Context.Device.Log.PrintDebug(LogClass.ServiceNv, "Waiting syncpt with timeout of " + Args.Timeout + "ms...");

                using (ManualResetEvent WaitEvent = new ManualResetEvent(false))
                {
                    Syncpt.AddWaiter(Args.Thresh, WaitEvent);

                    //Note: Negative (> INT_MAX) timeouts aren't valid on .NET,
                    //in this case we just use the maximum timeout possible.
                    int Timeout = Args.Timeout;

                    if (Timeout < -1)
                    {
                        Timeout = int.MaxValue;
                    }

                    if (Timeout == -1)
                    {
                        WaitEvent.WaitOne();

                        Result = NvResult.Success;
                    }
                    else if (WaitEvent.WaitOne(Timeout))
                    {
                        Result = NvResult.Success;
                    }
                    else
                    {
                        Result = NvResult.TimedOut;
                    }
                }

                Context.Device.Log.PrintDebug(LogClass.ServiceNv, "Resuming...");
            }

            if (Extended)
            {
                Context.Memory.WriteInt32(OutputPosition + 0xc, Syncpt.GetMin(Args.Id));
            }

            return Result;
        }

        private static int EventWait(ServiceCtx Context, bool Async)
        {
            long InputPosition  = Context.Request.GetBufferType0x21().Position;
            long OutputPosition = Context.Request.GetBufferType0x22().Position;

            NvHostCtrlSyncptWaitEx Args = AMemoryHelper.Read<NvHostCtrlSyncptWaitEx>(Context.Memory, InputPosition);

            if ((uint)Args.Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvResult.InvalidInput;
            }

            void WriteArgs()
            {
                AMemoryHelper.Write(Context.Memory, OutputPosition, Args);
            }

            NvHostSyncpt Syncpt = GetUserCtx(Context).Syncpt;

            if (Syncpt.MinCompare(Args.Id, Args.Thresh))
            {
                Args.Value = Syncpt.GetMin(Args.Id);

                WriteArgs();

                return NvResult.Success;
            }

            if (!Async)
            {
                Args.Value = 0;
            }

            if (Args.Timeout == 0)
            {
                WriteArgs();

                return NvResult.TryAgain;
            }

            NvHostEvent Event;

            int Result, EventIndex;

            if (Async)
            {
                EventIndex = Args.Value;

                if ((uint)EventIndex >= NvHostCtrlUserCtx.EventsCount)
                {
                    return NvResult.InvalidInput;
                }

                Event = GetUserCtx(Context).Events[EventIndex];
            }
            else
            {
                Event = GetFreeEvent(Context, Syncpt, Args.Id, out EventIndex);
            }

            if (Event != null &&
               (Event.State == NvHostEventState.Registered ||
                Event.State == NvHostEventState.Free))
            {
                Event.Id     = Args.Id;
                Event.Thresh = Args.Thresh;

                Event.State = NvHostEventState.Waiting;

                if (!Async)
                {
                    Args.Value = ((Args.Id & 0xfff) << 16) | 0x10000000;
                }
                else
                {
                    Args.Value = Args.Id << 4;
                }

                Args.Value |= EventIndex;

                Result = NvResult.TryAgain;
            }
            else
            {
                Result = NvResult.InvalidInput;
            }

            WriteArgs();

            return Result;
        }

        private static NvHostEvent GetFreeEvent(
            ServiceCtx   Context,
            NvHostSyncpt Syncpt,
            int          Id,
            out int      EventIndex)
        {
            NvHostEvent[] Events = GetUserCtx(Context).Events;

            EventIndex = NvHostCtrlUserCtx.EventsCount;

            int NullIndex = NvHostCtrlUserCtx.EventsCount;

            for (int Index = 0; Index < NvHostCtrlUserCtx.EventsCount; Index++)
            {
                NvHostEvent Event = Events[Index];

                if (Event != null)
                {
                    if (Event.State == NvHostEventState.Registered ||
                        Event.State == NvHostEventState.Free)
                    {
                        EventIndex = Index;

                        if (Event.Id == Id)
                        {
                            return Event;
                        }
                    }
                }
                else if (NullIndex == NvHostCtrlUserCtx.EventsCount)
                {
                    NullIndex = Index;
                }
            }

            if (NullIndex < NvHostCtrlUserCtx.EventsCount)
            {
                EventIndex = NullIndex;

                return Events[NullIndex] = new NvHostEvent();
            }

            if (EventIndex < NvHostCtrlUserCtx.EventsCount)
            {
                return Events[EventIndex];
            }

            return null;
        }

        public static NvHostCtrlUserCtx GetUserCtx(ServiceCtx Context)
        {
            return UserCtxs.GetOrAdd(Context.Process, (Key) => new NvHostCtrlUserCtx());
        }

        public static void UnloadProcess(Process Process)
        {
            UserCtxs.TryRemove(Process, out _);
        }
    }
}
