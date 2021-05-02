using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common
{
    struct AtomicStorage<T> where T: unmanaged
    {
        public ulong SamplingNumber;
        public T Object;

        public ulong ReadSamplingNumberAtomic()
        {
            return Interlocked.Read(ref SamplingNumber);
        }

        public void SetObject(ref T obj)
        {
            ISampledData samplingProvider = obj as ISampledData;

            Interlocked.Exchange(ref SamplingNumber, samplingProvider.SamplingNumber);

            Thread.MemoryBarrier();

            Object = obj;
        }
    }
}
