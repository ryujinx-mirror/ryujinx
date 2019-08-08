namespace ARMeilleure.Memory
{
    public interface IMemory
    {
        sbyte ReadSByte(long position);

        short ReadInt16(long position);

        int ReadInt32(long position);

        long ReadInt64(long position);

        byte ReadByte(long position);

        ushort ReadUInt16(long position);

        uint ReadUInt32(long position);

        ulong ReadUInt64(long position);

        void WriteSByte(long position, sbyte value);

        void WriteInt16(long position, short value);

        void WriteInt32(long position, int value);

        void WriteInt64(long position, long value);

        void WriteByte(long position, byte value);

        void WriteUInt16(long position, ushort value);

        void WriteUInt32(long position, uint value);

        void WriteUInt64(long position, ulong value);
    }
}