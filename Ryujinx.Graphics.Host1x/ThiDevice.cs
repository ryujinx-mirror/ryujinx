using Ryujinx.Common;
using Ryujinx.Graphics.Device;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Host1x
{
    class ThiDevice : IDeviceState, IDisposable
    {
        private readonly ClassId _classId;
        private readonly IDeviceState _device;

        private readonly SyncptIncrManager _syncptIncrMgr;

        private class CommandAction
        {
            public int Data { get; }

            public CommandAction(int data)
            {
                Data = data;
            }
        }

        private class MethodCallAction : CommandAction
        {
            public int Method { get; }

            public MethodCallAction(int method, int data) : base(data)
            {
                Method = method;
            }
        }

        private class SyncptIncrAction : CommandAction
        {
            public SyncptIncrAction(uint syncptIncrHandle) : base((int)syncptIncrHandle)
            {
            }
        }

        private readonly AsyncWorkQueue<CommandAction> _commandQueue;

        private readonly DeviceState<ThiRegisters> _state;

        public ThiDevice(ClassId classId, IDeviceState device, SyncptIncrManager syncptIncrMgr)
        {
            _classId = classId;
            _device = device;
            _syncptIncrMgr = syncptIncrMgr;
            _commandQueue = new AsyncWorkQueue<CommandAction>(Process, $"Ryujinx.{classId}Processor");
            _state = new DeviceState<ThiRegisters>(new Dictionary<string, RwCallback>
            {
                { nameof(ThiRegisters.IncrSyncpt), new RwCallback(IncrSyncpt, null) },
                { nameof(ThiRegisters.Method1), new RwCallback(Method1, null) }
            });
        }

        public int Read(int offset) => _state.Read(offset);
        public void Write(int offset, int data) => _state.Write(offset, data);

        private void IncrSyncpt(int data)
        {
            uint syncpointId = (uint)(data & 0xFF);
            uint cond = (uint)((data >> 8) & 0xFF); // 0 = Immediate, 1 = Done

            if (cond == 0)
            {
                _syncptIncrMgr.Increment(syncpointId);
            }
            else
            {
                _commandQueue.Add(new SyncptIncrAction(_syncptIncrMgr.IncrementWhenDone(_classId, syncpointId)));
            }
        }

        private void Method1(int data)
        {
            _commandQueue.Add(new MethodCallAction((int)_state.State.Method0 * 4, data));
        }

        private void Process(CommandAction cmdAction)
        {
            if (cmdAction is SyncptIncrAction syncptIncrAction)
            {
                _syncptIncrMgr.SignalDone((uint)syncptIncrAction.Data);
            }
            else if (cmdAction is MethodCallAction methodCallAction)
            {
                _device.Write(methodCallAction.Method, methodCallAction.Data);
            }
        }

        public void Dispose() => _commandQueue.Dispose();
    }
}
