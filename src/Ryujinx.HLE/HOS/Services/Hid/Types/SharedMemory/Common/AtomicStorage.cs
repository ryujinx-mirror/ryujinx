using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common
{
    struct AtomicStorage<T> where T : unmanaged, ISampledDataStruct
    {
        public ulong SamplingNumber;
        public T Object;

        public ulong ReadSamplingNumberAtomic()
        {
            return Interlocked.Read(ref SamplingNumber);
        }

        public void SetObject(ref T obj)
        {
            ulong samplingNumber = ISampledDataStruct.GetSamplingNumber(ref obj);

            Interlocked.Exchange(ref SamplingNumber, samplingNumber);

            Thread.MemoryBarrier();

            Object = obj;
        }
    }
}
