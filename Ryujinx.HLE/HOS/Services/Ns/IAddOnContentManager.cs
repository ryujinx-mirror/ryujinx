using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("aoc:u")]
    class IAddOnContentManager : IpcService
    {
        private readonly KEvent _addOnContentListChangedEvent;

        private int _addOnContentListChangedEventHandle;

        public IAddOnContentManager(ServiceCtx context)
        {
            _addOnContentListChangedEvent = new KEvent(context.Device.System.KernelContext);
        }

        [Command(2)]
        // CountAddOnContent(pid) -> u32
        public ResultCode CountAddOnContent(ServiceCtx context)
        {
            long pid = context.Process.Pid;

            // Official code checks ApplicationControlProperty.RuntimeAddOnContentInstall
            // if true calls ns:am ListAvailableAddOnContent again to get updated count

            byte runtimeAddOnContentInstall = context.Device.Application.ControlData.Value.RuntimeAddOnContentInstall;
            if (runtimeAddOnContentInstall != 0)
            {
                Logger.Warning?.Print(LogClass.ServiceNs, $"RuntimeAddOnContentInstall is true. Some DLC may be missing");
            }

            uint aocCount = CountAddOnContentImpl(context);

            context.ResponseData.Write(aocCount);

            Logger.Stub?.PrintStub(LogClass.ServiceNs, new { aocCount, runtimeAddOnContentInstall });

            return ResultCode.Success;
        }

        private static uint CountAddOnContentImpl(ServiceCtx context)
        {
            return (uint)context.Device.System.ContentManager.GetAocCount();
        }

        [Command(3)]
        // ListAddOnContent(u32, u32, pid) -> (u32, buffer<u32>)
        public ResultCode ListAddOnContent(ServiceCtx context)
        {
            uint startIndex = context.RequestData.ReadUInt32();
            uint bufferSize = context.RequestData.ReadUInt32();
            long pid = context.Process.Pid;

            var aocTitleIds = context.Device.System.ContentManager.GetAocTitleIds();

            uint aocCount = CountAddOnContentImpl(context);

            if (aocCount <= startIndex)
            {
                context.ResponseData.Write((uint)0);

                return ResultCode.Success;
            }

            aocCount = Math.Min(aocCount - startIndex, bufferSize);

            context.ResponseData.Write(aocCount);

            ulong bufAddr = (ulong)context.Request.ReceiveBuff[0].Position;

            ulong aocBaseId = GetAddOnContentBaseIdImpl(context);

            for (int i = 0; i < aocCount; ++i)
            {
                context.Memory.Write(bufAddr + (ulong)i * 4, (int)(aocTitleIds[i + (int)startIndex] - aocBaseId));
            }

            Logger.Stub?.PrintStub(LogClass.ServiceNs, new { bufferSize, startIndex, aocCount });

            return ResultCode.Success;
        }

        [Command(5)]
        // GetAddOnContentBaseId(pid) -> u64
        public ResultCode GetAddonContentBaseId(ServiceCtx context)
        {
            long pid = context.Process.Pid;

            // Official code calls arp:r GetApplicationControlProperty to get AddOnContentBaseId
            // If the call fails, calls arp:r GetApplicationLaunchProperty to get App TitleId
            ulong aocBaseId = GetAddOnContentBaseIdImpl(context);

            context.ResponseData.Write(aocBaseId);

            Logger.Stub?.PrintStub(LogClass.ServiceNs, $"aocBaseId={aocBaseId:X16}");

            // ResultCode will be error code of GetApplicationLaunchProperty if it fails
            return ResultCode.Success;
        }

        private static ulong GetAddOnContentBaseIdImpl(ServiceCtx context)
        {
            ulong aocBaseId = context.Device.Application.ControlData.Value.AddOnContentBaseId;

            if (aocBaseId == 0)
            {
                aocBaseId = context.Device.Application.TitleId + 0x1000;
            }

            return aocBaseId;
        }

        [Command(7)]
        // PrepareAddOnContent(u32, pid)
        public ResultCode PrepareAddOnContent(ServiceCtx context)
        {
            uint aocIndex = context.RequestData.ReadUInt32();
            long pid = context.Process.Pid;

            // Official Code calls a bunch of functions from arp:r for aocBaseId
            // and ns:am RegisterContentsExternalKey?, GetOwnedApplicationContentMetaStatus? etc...

            // Ideally, this should probably initialize the AocData values for the specified index

            Logger.Stub?.PrintStub(LogClass.ServiceNs, new { aocIndex });

            return ResultCode.Success;
        }

        [Command(8)]
        // GetAddOnContentListChangedEvent() -> handle<copy>
        public ResultCode GetAddOnContentListChangedEvent(ServiceCtx context)
        {
            // Official code seems to make an internal call to ns:am Cmd 84 GetDynamicCommitEvent()

            if (_addOnContentListChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_addOnContentListChangedEvent.ReadableEvent, out _addOnContentListChangedEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_addOnContentListChangedEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }


        [Command(9)] // [10.0.0+]
        // GetAddOnContentLostErrorCode() -> u64
        public ResultCode GetAddOnContentLostErrorCode(ServiceCtx context)
        {
            // Seems to calculate ((value & 0x1ff)) + 2000 on 0x7d0a4
            // which gives 0x874 (2000+164). 164 being Module ID of `EC (Shop)`
            context.ResponseData.Write(2164L);

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [Command(100)]
        // CreateEcPurchasedEventManager() -> object<nn::ec::IPurchaseEventManager>
        public ResultCode CreateEcPurchasedEventManager(ServiceCtx context)
        {
            MakeObject(context, new IPurchaseEventManager(context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [Command(101)]
        // CreatePermanentEcPurchasedEventManager() -> object<nn::ec::IPurchaseEventManager>
        public ResultCode CreatePermanentEcPurchasedEventManager(ServiceCtx context)
        {
            // Very similar to CreateEcPurchasedEventManager but with some extra code

            MakeObject(context, new IPurchaseEventManager(context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }
    }
}