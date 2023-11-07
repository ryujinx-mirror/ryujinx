using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Ryujinx.Cpu.AppleHv
{
    static class HvCodePatcher
    {
        private const uint XMask = 0x3f808000u;
        private const uint XValue = 0x8000000u;

        private const uint ZrIndex = 31u;

        public static void RewriteUnorderedExclusiveInstructions(Span<byte> code)
        {
            Span<uint> codeUint = MemoryMarshal.Cast<byte, uint>(code);
            Span<Vector128<uint>> codeVector = MemoryMarshal.Cast<byte, Vector128<uint>>(code);

            Vector128<uint> mask = Vector128.Create(XMask);
            Vector128<uint> value = Vector128.Create(XValue);

            for (int index = 0; index < codeVector.Length; index++)
            {
                Vector128<uint> v = codeVector[index];

                if (Vector128.EqualsAny(Vector128.BitwiseAnd(v, mask), value))
                {
                    int baseIndex = index * 4;

                    for (int instIndex = baseIndex; instIndex < baseIndex + 4; instIndex++)
                    {
                        ref uint inst = ref codeUint[instIndex];

                        if ((inst & XMask) != XValue)
                        {
                            continue;
                        }

                        bool isPair = (inst & (1u << 21)) != 0;
                        bool isLoad = (inst & (1u << 22)) != 0;

                        uint rt2 = (inst >> 10) & 0x1fu;
                        uint rs = (inst >> 16) & 0x1fu;

                        if (isLoad && rs != ZrIndex)
                        {
                            continue;
                        }

                        if (!isPair && rt2 != ZrIndex)
                        {
                            continue;
                        }

                        // Set the ordered flag.
                        inst |= 1u << 15;
                    }
                }
            }
        }
    }
}
