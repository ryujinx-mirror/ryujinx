using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Synchronization;
using System;
using System.Numerics;

namespace Ryujinx.Graphics.Host1x
{
    public sealed class Host1xDevice : IDisposable
    {
        private readonly SyncptIncrManager _syncptIncrMgr;
        private readonly AsyncWorkQueue<int[]> _commandQueue;

        private readonly Devices _devices = new Devices();

        public Host1xClass Class { get; }

        private IDeviceState _device;

        private int _count;
        private int _offset;
        private int _mask;
        private bool _incrementing;

        public Host1xDevice(SynchronizationManager syncMgr)
        {
            _syncptIncrMgr = new SyncptIncrManager(syncMgr);
            _commandQueue = new AsyncWorkQueue<int[]>(Process, "Ryujinx.Host1xProcessor");

            Class = new Host1xClass(syncMgr);

            _devices.RegisterDevice(ClassId.Host1x, Class);
        }

        public void RegisterDevice(ClassId classId, IDeviceState device)
        {
            var thi = new ThiDevice(classId, device ?? throw new ArgumentNullException(nameof(device)), _syncptIncrMgr);
            _devices.RegisterDevice(classId, thi);
        }

        public void Submit(ReadOnlySpan<int> commandBuffer)
        {
            _commandQueue.Add(commandBuffer.ToArray());
        }

        private void Process(int[] commandBuffer)
        {
            for (int index = 0; index < commandBuffer.Length; index++)
            {
                Step(commandBuffer[index]);
            }
        }

        private void Step(int value)
        {
            if (_mask != 0)
            {
                int lbs = BitOperations.TrailingZeroCount(_mask);

                _mask &= ~(1 << lbs);

                DeviceWrite(_offset + lbs, value);

                return;
            }
            else if (_count != 0)
            {
                _count--;

                DeviceWrite(_offset, value);

                if (_incrementing)
                {
                    _offset++;
                }

                return;
            }

            OpCode opCode = (OpCode)((value >> 28) & 0xf);

            switch (opCode)
            {
                case OpCode.SetClass:
                    _mask = value & 0x3f;
                    ClassId classId = (ClassId)((value >> 6) & 0x3ff);
                    _offset = (value >> 16) & 0xfff;
                    _device = _devices.GetDevice(classId);
                    break;
                case OpCode.Incr:
                case OpCode.NonIncr:
                    _count = value & 0xffff;
                    _offset = (value >> 16) & 0xfff;
                    _incrementing = opCode == OpCode.Incr;
                    break;
                case OpCode.Mask:
                    _mask = value & 0xffff;
                    _offset = (value >> 16) & 0xfff;
                    break;
                case OpCode.Imm:
                    int data = value & 0xfff;
                    _offset = (value >> 16) & 0xfff;
                    DeviceWrite(_offset, data);
                    break;
                default:
                    Logger.Error?.Print(LogClass.Host1x, $"Unsupported opcode \"{opCode}\".");
                    break;
            }
        }

        private void DeviceWrite(int offset, int data)
        {
            _device?.Write(offset * 4, data);
        }

        public void Dispose()
        {
            _commandQueue.Dispose();
            _devices.Dispose();
        }
    }
}
