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

        private static readonly TableEntry[] Table = new TableEntry[]
        {
            new TableEntry(MacroHLEFunctionName.MultiDrawElementsIndirectCount, new Hash128(0x890AF57ED3FB1C37, 0x35D0C95C61F5386F), 0x19C)
        };

        private static bool IsMacroHLESupported(Capabilities caps, MacroHLEFunctionName name)
        {
            if (name == MacroHLEFunctionName.MultiDrawElementsIndirectCount)
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

            for (int i = 0; i < Table.Length; i++)
            {
                ref var entry = ref Table[i];

                var hash = XXHash128.ComputeHash(mc.Slice(0, entry.Length));
                if (hash == entry.Hash)
                {
                    name = entry.Name;
                    return IsMacroHLESupported(caps, name);
                }
            }

            name = MacroHLEFunctionName.None;
            return false;
        }
    }
}
