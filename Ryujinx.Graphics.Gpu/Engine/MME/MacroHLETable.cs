using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Table with information about High-level implementations of GPU Macro code.
    /// </summary>
    static class MacroHLETable
    {
        /// <summary>
        /// Macroo High-level implementation table entry.
        /// </summary>
        struct TableEntry
        {
            /// <summary>
            /// Name of the Macro function.
            /// </summary>
            public MacroHLEFunctionName Name { get; }

            /// <summary>
            /// Hash of the original binary Macro function code.
            /// </summary>
            public Hash128 Hash { get; }

            /// <summary>
            /// Size (in bytes) of the original binary Macro function code.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// Creates a new table entry.
            /// </summary>
            /// <param name="name">Name of the Macro function</param>
            /// <param name="hash">Hash of the original binary Macro function code</param>
            /// <param name="length">Size (in bytes) of the original binary Macro function code</param>
            public TableEntry(MacroHLEFunctionName name, Hash128 hash, int length)
            {
                Name = name;
                Hash = hash;
                Length = length;
            }
        }

        private static readonly TableEntry[] _table = new TableEntry[]
        {
            new TableEntry(MacroHLEFunctionName.ClearColor, new Hash128(0xA9FB28D1DC43645A, 0xB177E5D2EAE67FB0), 0x28),
            new TableEntry(MacroHLEFunctionName.ClearDepthStencil, new Hash128(0x1B96CB77D4879F4F, 0x8557032FE0C965FB), 0x24),
            new TableEntry(MacroHLEFunctionName.DrawArraysInstanced, new Hash128(0x197FB416269DBC26, 0x34288C01DDA82202), 0x48),
            new TableEntry(MacroHLEFunctionName.DrawElementsInstanced, new Hash128(0x1A501FD3D54EC8E0, 0x6CF570CF79DA74D6), 0x5c),
            new TableEntry(MacroHLEFunctionName.DrawElementsIndirect, new Hash128(0x86A3E8E903AF8F45, 0xD35BBA07C23860A4), 0x7c),
            new TableEntry(MacroHLEFunctionName.MultiDrawElementsIndirectCount, new Hash128(0x890AF57ED3FB1C37, 0x35D0C95C61F5386F), 0x19C)
        };

        /// <summary>
        /// Checks if the host supports all features required by the HLE macro.
        /// </summary>
        /// <param name="caps">Host capabilities</param>
        /// <param name="name">Name of the HLE macro to be checked</param>
        /// <returns>True if the host supports the HLE macro, false otherwise</returns>
        private static bool IsMacroHLESupported(Capabilities caps, MacroHLEFunctionName name)
        {
            if (name == MacroHLEFunctionName.ClearColor ||
                name == MacroHLEFunctionName.ClearDepthStencil ||
                name == MacroHLEFunctionName.DrawArraysInstanced ||
                name == MacroHLEFunctionName.DrawElementsInstanced ||
                name == MacroHLEFunctionName.DrawElementsIndirect)
            {
                return true;
            }
            else if (name == MacroHLEFunctionName.MultiDrawElementsIndirectCount)
            {
                return caps.SupportsIndirectParameters;
            }

            return false;
        }

        /// <summary>
        /// Checks if there's a fast, High-level implementation of the specified Macro code available.
        /// </summary>
        /// <param name="code">Macro code to be checked</param>
        /// <param name="caps">Renderer capabilities to check for this macro HLE support</param>
        /// <param name="name">Name of the function if a implementation is available and supported, otherwise <see cref="MacroHLEFunctionName.None"/></param>
        /// <returns>True if there is a implementation available and supported, false otherwise</returns>
        public static bool TryGetMacroHLEFunction(ReadOnlySpan<int> code, Capabilities caps, out MacroHLEFunctionName name)
        {
            var mc = MemoryMarshal.Cast<int, byte>(code);

            for (int i = 0; i < _table.Length; i++)
            {
                ref var entry = ref _table[i];

                var hash = XXHash128.ComputeHash(mc.Slice(0, entry.Length));
                if (hash == entry.Hash)
                {
                    if (IsMacroHLESupported(caps, entry.Name))
                    {
                        name = entry.Name;
                        return true;
                    }

                    break;
                }
            }

            name = MacroHLEFunctionName.None;
            return false;
        }
    }
}
