using System;
namespace SoundIOSharp
{
    public class SoundIORingBuffer : IDisposable
    {
        internal SoundIORingBuffer(IntPtr handle)
        {
            this.handle = handle;
        }

        IntPtr handle;

        public int Capacity
        {
            get { return Natives.soundio_ring_buffer_capacity(handle); }
        }

        public void Clear()
        {
            Natives.soundio_ring_buffer_clear(handle);
        }

        public void Dispose()
        {
            Natives.soundio_ring_buffer_destroy(handle);
        }

        public int FillCount
        {
            get { return Natives.soundio_ring_buffer_fill_count(handle); }
        }

        public int FreeCount
        {
            get { return Natives.soundio_ring_buffer_free_count(handle); }
        }

        public IntPtr ReadPointer
        {
            get { return Natives.soundio_ring_buffer_read_ptr(handle); }
        }

        public IntPtr WritePointer
        {
            get { return Natives.soundio_ring_buffer_write_ptr(handle); }
        }

        public void AdvanceReadPointer(int count)
        {
            Natives.soundio_ring_buffer_advance_read_ptr(handle, count);
        }

        public void AdvanceWritePointer(int count)
        {
            Natives.soundio_ring_buffer_advance_write_ptr(handle, count);
        }
    }
}