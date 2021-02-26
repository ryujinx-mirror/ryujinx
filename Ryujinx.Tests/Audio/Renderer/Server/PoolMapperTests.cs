using NUnit.Framework;
using Ryujinx.Audio;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;
using CpuAddress = System.UInt64;
using DspAddress = System.UInt64;

namespace Ryujinx.Tests.Audio.Renderer.Server
{
    class PoolMapperTests
    {
        private const uint DummyProcessHandle = 0xCAFEBABE;

        [Test]
        public void TestInitializeSystemPool()
        {
            PoolMapper      poolMapper    = new PoolMapper(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong      CpuSize    = 0x1000;

            Assert.IsFalse(poolMapper.InitializeSystemPool(ref memoryPoolCpu, CpuAddress, CpuSize));
            Assert.IsTrue(poolMapper.InitializeSystemPool(ref memoryPoolDsp, CpuAddress, CpuSize));

            Assert.AreEqual(CpuAddress, memoryPoolDsp.CpuAddress);
            Assert.AreEqual(CpuSize, memoryPoolDsp.Size);
            Assert.AreEqual(DspAddress, memoryPoolDsp.DspAddress);
        }

        [Test]
        public void TestGetProcessHandle()
        {
            PoolMapper      poolMapper    = new PoolMapper(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            Assert.AreEqual(0xFFFF8001, poolMapper.GetProcessHandle(ref memoryPoolCpu));
            Assert.AreEqual(DummyProcessHandle, poolMapper.GetProcessHandle(ref memoryPoolDsp));
        }

        [Test]
        public void TestMappings()
        {
            PoolMapper      poolMapper    = new PoolMapper(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong      CpuSize    = 0x1000;

            memoryPoolDsp.SetCpuAddress(CpuAddress, CpuSize);
            memoryPoolCpu.SetCpuAddress(CpuAddress, CpuSize);

            Assert.AreEqual(DspAddress, poolMapper.Map(ref memoryPoolCpu));
            Assert.AreEqual(DspAddress, poolMapper.Map(ref memoryPoolDsp));
            Assert.AreEqual(DspAddress, memoryPoolDsp.DspAddress);
            Assert.IsTrue(poolMapper.Unmap(ref memoryPoolCpu));

            memoryPoolDsp.IsUsed = true;
            Assert.IsFalse(poolMapper.Unmap(ref memoryPoolDsp));
            memoryPoolDsp.IsUsed = false;
            Assert.IsTrue(poolMapper.Unmap(ref memoryPoolDsp));
        }

        [Test]
        public void TestTryAttachBuffer()
        {
            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong      CpuSize    = 0x1000;

            const int        MemoryPoolStateArraySize = 0x10;
            const CpuAddress CpuAddressRegionEnding   = CpuAddress * MemoryPoolStateArraySize;

            MemoryPoolState[] memoryPoolStateArray = new MemoryPoolState[MemoryPoolStateArraySize];

            for (int i = 0; i < memoryPoolStateArray.Length; i++)
            {
                memoryPoolStateArray[i] = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);
                memoryPoolStateArray[i].SetCpuAddress(CpuAddress + (ulong)i * CpuSize, CpuSize);
            }

            ErrorInfo errorInfo;

            AddressInfo addressInfo = AddressInfo.Create();

            PoolMapper poolMapper = new PoolMapper(DummyProcessHandle, true);

            Assert.IsTrue(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, 0, 0));

            Assert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            Assert.AreEqual(0, errorInfo.ExtraErrorInfo);
            Assert.AreEqual(0, addressInfo.ForceMappedDspAddress);

            Assert.IsTrue(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            Assert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            Assert.AreEqual(CpuAddress, errorInfo.ExtraErrorInfo);
            Assert.AreEqual(DspAddress, addressInfo.ForceMappedDspAddress);

            poolMapper = new PoolMapper(DummyProcessHandle, false);

            Assert.IsFalse(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, 0, 0));

            addressInfo.ForceMappedDspAddress = 0;

            Assert.IsFalse(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            Assert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            Assert.AreEqual(CpuAddress, errorInfo.ExtraErrorInfo);
            Assert.AreEqual(0, addressInfo.ForceMappedDspAddress);

            poolMapper = new PoolMapper(DummyProcessHandle, memoryPoolStateArray.AsMemory(), false);

            Assert.IsFalse(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddressRegionEnding, CpuSize));

            Assert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            Assert.AreEqual(CpuAddressRegionEnding, errorInfo.ExtraErrorInfo);
            Assert.AreEqual(0, addressInfo.ForceMappedDspAddress);
            Assert.IsFalse(addressInfo.HasMemoryPoolState);

            Assert.IsTrue(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            Assert.AreEqual(ResultCode.Success, errorInfo.ErrorCode);
            Assert.AreEqual(0, errorInfo.ExtraErrorInfo);
            Assert.AreEqual(0, addressInfo.ForceMappedDspAddress);
            Assert.IsTrue(addressInfo.HasMemoryPoolState);
        }
    }
}
