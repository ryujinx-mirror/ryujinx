using Ryujinx.Common;

namespace Ryujinx.Horizon.Sdk.Sf
{
    static class RawDataOffsetCalculator
    {
        public static int[] Calculate(CommandArg[] args)
        {
            int[] offsets = new int[args.Length + 1];

            if (args.Length != 0)
            {
                int argsCount = args.Length;

                int[] sizes = new int[argsCount];
                int[] aligns = new int[argsCount];
                int[] map = new int[argsCount];

                for (int i = 0; i < argsCount; i++)
                {
                    sizes[i] = args[i].ArgSize;
                    aligns[i] = args[i].ArgAlignment;
                    map[i] = i;
                }

                for (int i = 1; i < argsCount; i++)
                {
                    for (int j = i; j > 0 && aligns[map[j - 1]] > aligns[map[j]]; j--)
                    {
                        (map[j], map[j - 1]) = (map[j - 1], map[j]);
                    }
                }

                int offset = 0;

                foreach (int i in map)
                {
                    offset = BitUtils.AlignUp(offset, aligns[i]);
                    offsets[i] = offset;
                    offset += sizes[i];
                }

                offsets[argsCount] = offset;
            }

            return offsets;
        }
    }
}
