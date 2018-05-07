namespace ChocolArm64.Memory
{
    public interface IAMemory
    {
        sbyte ReadSByte(long Position);

        short ReadInt16(long Position);

        int ReadInt32(long Position);

        long ReadInt64(long Position);

        byte ReadByte(long Position);

        ushort ReadUInt16(long Position);

        uint ReadUInt32(long Position);

        ulong ReadUInt64(long Position);

        void WriteSByte(long Position, sbyte Value);

        void WriteInt16(long Position, short Value);

        void WriteInt32(long Position, int Value);

        void WriteInt64(long Position, long Value);

        void WriteByte(long Position, byte Value);

        void WriteUInt16(long Position, ushort Value);

        void WriteUInt32(long Position, uint Value);

        void WriteUInt64(long Position, ulong Value);
    }
}