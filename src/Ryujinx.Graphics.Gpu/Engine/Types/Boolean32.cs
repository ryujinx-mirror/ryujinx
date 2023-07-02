namespace Ryujinx.Graphics.Gpu.Engine.Types
{
    /// <summary>
    /// Boolean value, stored as a 32-bits integer in memory.
    /// </summary>
    readonly struct Boolean32
    {
#pragma warning disable CS0649 // Field is never assigned to
        private readonly uint _value;
#pragma warning restore CS0649

        public static implicit operator bool(Boolean32 value)
        {
            return (value._value & 1) != 0;
        }
    }
}
