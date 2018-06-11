using ChocolArm64.Memory;

namespace Ryujinx.HLE.OsHle
{
    static class Homebrew
    {
        //http://switchbrew.org/index.php?title=Homebrew_ABI
        public static void WriteHbAbiData(AMemory Memory, long Position, int MainThreadHandle)
        {
            Memory.Manager.Map(Position, AMemoryMgr.PageSize, (int)MemoryType.Normal, AMemoryPerm.RW);

            //MainThreadHandle
            WriteConfigEntry(Memory, ref Position, 1, 0, MainThreadHandle);

            //NextLoadPath
            WriteConfigEntry(Memory, ref Position, 2, 0, Position + 0x200, Position + 0x400);

            //AppletType
            WriteConfigEntry(Memory, ref Position, 7);

            //EndOfList
            WriteConfigEntry(Memory, ref Position, 0);
        }

        private static void WriteConfigEntry(
            AMemory  Memory,
            ref long Position,
            int      Key,
            int      Flags  = 0,
            long     Value0 = 0,
            long     Value1 = 0)
        {
            Memory.WriteInt32(Position + 0x00, Key);
            Memory.WriteInt32(Position + 0x04, Flags);
            Memory.WriteInt64(Position + 0x08, Value0);
            Memory.WriteInt64(Position + 0x10, Value1);

            Position += 0x18;
        }

        public static string ReadHbAbiNextLoadPath(AMemory Memory, long Position)
        {
            string FileName = null;

            while (true)
            {
                long Key = Memory.ReadInt64(Position);

                if (Key == 2)
                {
                    long Value0 = Memory.ReadInt64(Position + 0x08);
                    long Value1 = Memory.ReadInt64(Position + 0x10);

                    FileName = AMemoryHelper.ReadAsciiString(Memory, Value0, Value1 - Value0);

                    break;
                }
                else if (Key == 0)
                {
                    break;
                }

                Position += 0x18;
            }

            return FileName;
        }
    }
}
