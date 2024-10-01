using Ryujinx.Cpu.LightningJit.Table;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct InstInfoForTable : IInstInfo
    {
        public uint Encoding { get; }
        public uint EncodingMask { get; }
        public InstEncoding[] Constraints { get; }
        public InstMeta Meta { get; }
        public IsaVersion Version => Meta.Version;
        public IsaFeature Feature => Meta.Feature;

        public InstInfoForTable(
            uint encoding,
            uint encodingMask,
            InstEncoding[] constraints,
            InstName name,
            Action<CodeGenContext, uint> emitFunc,
            IsaVersion isaVersion,
            IsaFeature isaFeature,
            InstFlags flags)
        {
            Encoding = encoding;
            EncodingMask = encodingMask;
            Constraints = constraints;
            Meta = new(name, emitFunc, isaVersion, isaFeature, flags);
        }

        public InstInfoForTable(
            uint encoding,
            uint encodingMask,
            InstEncoding[] constraints,
            InstName name,
            Action<CodeGenContext, uint> emitFunc,
            IsaVersion isaVersion,
            InstFlags flags) : this(encoding, encodingMask, constraints, name, emitFunc, isaVersion, IsaFeature.None, flags)
        {
        }

        public InstInfoForTable(
            uint encoding,
            uint encodingMask,
            InstName name,
            Action<CodeGenContext, uint> emitFunc,
            IsaVersion isaVersion,
            IsaFeature isaFeature,
            InstFlags flags) : this(encoding, encodingMask, null, name, emitFunc, isaVersion, isaFeature, flags)
        {
        }

        public InstInfoForTable(
            uint encoding,
            uint encodingMask,
            InstName name,
            Action<CodeGenContext, uint> emitFunc,
            IsaVersion isaVersion,
            InstFlags flags) : this(encoding, encodingMask, null, name, emitFunc, isaVersion, IsaFeature.None, flags)
        {
        }

        public bool IsConstrained(uint encoding)
        {
            if (Constraints != null)
            {
                foreach (InstEncoding constraint in Constraints)
                {
                    if ((encoding & constraint.EncodingMask) == constraint.Encoding)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
