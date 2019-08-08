using ARMeilleure.IntermediateRepresentation;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class JitUnwindWindows
    {
        private const int MaxUnwindCodesArraySize = 9 + 10 * 2 + 3;

        private struct RuntimeFunction
        {
            public uint BeginAddress;
            public uint EndAddress;
            public uint UnwindData;
        }

        private struct UnwindInfo
        {
            public byte VersionAndFlags;
            public byte SizeOfProlog;
            public byte CountOfUnwindCodes;
            public byte FrameRegister;
            public unsafe fixed ushort UnwindCodes[MaxUnwindCodesArraySize];
        }

        private enum UnwindOperation
        {
            PushNonvol    = 0,
            AllocLarge    = 1,
            AllocSmall    = 2,
            SetFpreg      = 3,
            SaveNonvol    = 4,
            SaveNonvolFar = 5,
            SaveXmm128    = 8,
            SaveXmm128Far = 9,
            PushMachframe = 10
        }

        private unsafe delegate RuntimeFunction* GetRuntimeFunctionCallback(ulong controlPc, IntPtr context);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static unsafe extern bool RtlInstallFunctionTableCallback(
            ulong                      tableIdentifier,
            ulong                      baseAddress,
            uint                       length,
            GetRuntimeFunctionCallback callback,
            IntPtr                     context,
            string                     outOfProcessCallbackDll);

        private static GetRuntimeFunctionCallback _getRuntimeFunctionCallback;

        private static int _sizeOfRuntimeFunction;

        private unsafe static RuntimeFunction* _runtimeFunction;

        private unsafe static UnwindInfo* _unwindInfo;

        public static void InstallFunctionTableHandler(IntPtr codeCachePointer, uint codeCacheLength)
        {
            ulong codeCachePtr = (ulong)codeCachePointer.ToInt64();

            _sizeOfRuntimeFunction = Marshal.SizeOf<RuntimeFunction>();

            bool result;

            unsafe
            {
                _runtimeFunction = (RuntimeFunction*)codeCachePointer;

                _unwindInfo = (UnwindInfo*)(codeCachePointer + _sizeOfRuntimeFunction);

                _getRuntimeFunctionCallback = new GetRuntimeFunctionCallback(FunctionTableHandler);

                result = RtlInstallFunctionTableCallback(
                    codeCachePtr | 3,
                    codeCachePtr,
                    codeCacheLength,
                    _getRuntimeFunctionCallback,
                    codeCachePointer,
                    null);
            }

            if (!result)
            {
                throw new InvalidOperationException("Failure installing function table callback.");
            }
        }

        private static unsafe RuntimeFunction* FunctionTableHandler(ulong controlPc, IntPtr context)
        {
            int offset = (int)((long)controlPc - context.ToInt64());

            if (!JitCache.TryFind(offset, out JitCacheEntry funcEntry))
            {
                // Not found.
                return null;
            }

            var unwindInfo = funcEntry.UnwindInfo;

            int codeIndex = 0;

            int spOffset = unwindInfo.FixedAllocSize;

            foreach (var entry in unwindInfo.PushEntries)
            {
                if (entry.Type == RegisterType.Vector)
                {
                    spOffset -= 16;
                }
            }

            for (int index = unwindInfo.PushEntries.Length - 1; index >= 0; index--)
            {
                var entry = unwindInfo.PushEntries[index];

                if (entry.Type == RegisterType.Vector)
                {
                    ushort uwop = PackUwop(UnwindOperation.SaveXmm128, entry.StreamEndOffset, entry.Index);

                    _unwindInfo->UnwindCodes[codeIndex++] = uwop;
                    _unwindInfo->UnwindCodes[codeIndex++] = (ushort)spOffset;

                    spOffset += 16;
                }
            }

            _unwindInfo->UnwindCodes[0] = PackUwop(UnwindOperation.AllocLarge, unwindInfo.PrologueSize, 1);
            _unwindInfo->UnwindCodes[1] = (ushort)(unwindInfo.FixedAllocSize >> 0);
            _unwindInfo->UnwindCodes[2] = (ushort)(unwindInfo.FixedAllocSize >> 16);

            codeIndex += 3;

            for (int index = unwindInfo.PushEntries.Length - 1; index >= 0; index--)
            {
                var entry = unwindInfo.PushEntries[index];

                if (entry.Type == RegisterType.Integer)
                {
                    ushort uwop = PackUwop(UnwindOperation.PushNonvol, entry.StreamEndOffset, entry.Index);

                    _unwindInfo->UnwindCodes[codeIndex++] = uwop;
                }
            }

            _unwindInfo->VersionAndFlags    = 1;
            _unwindInfo->SizeOfProlog       = (byte)unwindInfo.PrologueSize;
            _unwindInfo->CountOfUnwindCodes = (byte)codeIndex;
            _unwindInfo->FrameRegister      = 0;

            _runtimeFunction->BeginAddress = (uint)funcEntry.Offset;
            _runtimeFunction->EndAddress   = (uint)(funcEntry.Offset + funcEntry.Size);
            _runtimeFunction->UnwindData   = (uint)_sizeOfRuntimeFunction;

            return _runtimeFunction;
        }

        private static ushort PackUwop(UnwindOperation uwop, int prologOffset, int opInfo)
        {
            return (ushort)(prologOffset | ((int)uwop << 8) | (opInfo << 12));
        }
    }
}