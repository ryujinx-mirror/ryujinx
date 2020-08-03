using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Mm.Types;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Mm
{
    [Service("mm:u")]
    class IRequest : IpcService
    {
        private static object                  _sessionListLock = new object();
        private static List<MultiMediaSession> _sessionList     = new List<MultiMediaSession>();

        private static uint _uniqueId = 1;

        public IRequest(ServiceCtx context) {}

        [Command(0)]
        // InitializeOld(u32, u32, u32)
        public ResultCode InitializeOld(ServiceCtx context)
        {
            MultiMediaOperationType operationType    = (MultiMediaOperationType)context.RequestData.ReadUInt32();
            int                     fgmId            = context.RequestData.ReadInt32();
            bool                    isAutoClearEvent = context.RequestData.ReadInt32() != 0;

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { operationType, fgmId, isAutoClearEvent });

            Register(operationType, fgmId, isAutoClearEvent);

            return ResultCode.Success;
        }

        [Command(1)]
        // FinalizeOld(u32)
        public ResultCode FinalizeOld(ServiceCtx context)
        {
            MultiMediaOperationType operationType = (MultiMediaOperationType)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { operationType });

            lock (_sessionListLock)
            {
                _sessionList.Remove(GetSessionByType(operationType));
            }

            return ResultCode.Success;
        }

        [Command(2)]
        // SetAndWaitOld(u32, u32, u32)
        public ResultCode SetAndWaitOld(ServiceCtx context)
        {
            MultiMediaOperationType operationType = (MultiMediaOperationType)context.RequestData.ReadUInt32();
            uint                    value         = context.RequestData.ReadUInt32();
            int                     timeout       = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { operationType, value, timeout });

            lock (_sessionListLock)
            {
                GetSessionByType(operationType)?.SetAndWait(value, timeout);
            }

            return ResultCode.Success;
        }

        [Command(3)]
        // GetOld(u32) -> u32
        public ResultCode GetOld(ServiceCtx context)
        {
            MultiMediaOperationType operationType = (MultiMediaOperationType)context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { operationType });

            lock (_sessionListLock)
            {
                MultiMediaSession session = GetSessionByType(operationType);

                uint currentValue = session == null ? 0 : session.CurrentValue;

                context.ResponseData.Write(currentValue);
            }

            return ResultCode.Success;
        }

        [Command(4)]
        // Initialize(u32, u32, u32) -> u32
        public ResultCode Initialize(ServiceCtx context)
        {
            MultiMediaOperationType operationType    = (MultiMediaOperationType)context.RequestData.ReadUInt32();
            int                     fgmId            = context.RequestData.ReadInt32();
            bool                    isAutoClearEvent = context.RequestData.ReadInt32() != 0;

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { operationType, fgmId, isAutoClearEvent });

            uint id = Register(operationType, fgmId, isAutoClearEvent);

            context.ResponseData.Write(id);

            return ResultCode.Success;
        }

        [Command(5)]
        // Finalize(u32)
        public ResultCode Finalize(ServiceCtx context)
        {
            uint id = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { id });

            lock (_sessionListLock)
            {
                _sessionList.Remove(GetSessionById(id));
            }

            return ResultCode.Success;
        }

        [Command(6)]
        // SetAndWait(u32, u32, u32)
        public ResultCode SetAndWait(ServiceCtx context)
        {
            uint id      = context.RequestData.ReadUInt32();
            uint value   = context.RequestData.ReadUInt32();
            int  timeout = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { id, value, timeout });

            lock (_sessionListLock)
            {
                GetSessionById(id)?.SetAndWait(value, timeout);
            }

            return ResultCode.Success;
        }

        [Command(7)]
        // Get(u32) -> u32
        public ResultCode Get(ServiceCtx context)
        {
            uint id = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceMm, new { id });

            lock (_sessionListLock)
            {
                MultiMediaSession session = GetSessionById(id);

                uint currentValue = session == null ? 0 : session.CurrentValue;

                context.ResponseData.Write(currentValue);
            }

            return ResultCode.Success;
        }

        private MultiMediaSession GetSessionById(uint id)
        {
            foreach (MultiMediaSession session in _sessionList)
            {
                if (session.Id == id)
                {
                    return session;
                }
            }

            return null;
        }

        private MultiMediaSession GetSessionByType(MultiMediaOperationType type)
        {
            foreach (MultiMediaSession session in _sessionList)
            {
                if (session.Type == type)
                {
                    return session;
                }
            }

            return null;
        }

        private uint Register(MultiMediaOperationType type, int fgmId, bool isAutoClearEvent)
        {
            lock (_sessionListLock)
            {
                // Nintendo ignore the fgm id as the other interfaces were deprecated.
                MultiMediaSession session = new MultiMediaSession(_uniqueId++, type, isAutoClearEvent);

                _sessionList.Add(session);

                return session.Id;
            }
        }
    }
}