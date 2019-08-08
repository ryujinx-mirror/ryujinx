using ARMeilleure.Memory;
using System.Text;

namespace Ryujinx.HLE.HOS
{
    static class Homebrew
    {
        public const string TemporaryNroSuffix = ".ryu_tmp.nro";

        // http://switchbrew.org/index.php?title=Homebrew_ABI
        public static void WriteHbAbiData(IMemoryManager memory, long position, int mainThreadHandle, string switchPath)
        {
            // MainThreadHandle.
            WriteConfigEntry(memory, ref position, 1, 0, mainThreadHandle);

            // NextLoadPath.
            WriteConfigEntry(memory, ref position, 2, 0, position + 0x200, position + 0x400);

            // Argv.
            long argvPosition = position + 0xC00;

            memory.WriteBytes(argvPosition, Encoding.ASCII.GetBytes(switchPath + "\0"));

            WriteConfigEntry(memory, ref position, 5, 0, 0, argvPosition);

            // AppletType.
            WriteConfigEntry(memory, ref position, 7);

            // EndOfList.
            WriteConfigEntry(memory, ref position, 0);
        }

        private static void WriteConfigEntry(
            IMemoryManager memory,
            ref long       position,
            int            key,
            int            flags  = 0,
            long           value0 = 0,
            long           value1 = 0)
        {
            memory.WriteInt32(position + 0x00, key);
            memory.WriteInt32(position + 0x04, flags);
            memory.WriteInt64(position + 0x08, value0);
            memory.WriteInt64(position + 0x10, value1);

            position += 0x18;
        }

        public static string ReadHbAbiNextLoadPath(IMemoryManager memory, long position)
        {
            string fileName = null;

            while (true)
            {
                long key = memory.ReadInt64(position);

                if (key == 2)
                {
                    long value0 = memory.ReadInt64(position + 0x08);
                    long value1 = memory.ReadInt64(position + 0x10);

                    fileName = MemoryHelper.ReadAsciiString(memory, value0, value1 - value0);

                    break;
                }
                else if (key == 0)
                {
                    break;
                }

                position += 0x18;
            }

            return fileName;
        }
    }
}
