using System.Collections.Generic;

namespace Ryujinx.HLE.Loaders.Npdm
{
    struct KernelAccessControlItem
    {
        public bool HasKernelFlags        { get; set; }
        public uint LowestThreadPriority  { get; set; }
        public uint HighestThreadPriority { get; set; }
        public uint LowestCpuId           { get; set; }
        public uint HighestCpuId          { get; set; }

        public bool   HasSvcFlags { get; set; }
        public bool[] AllowedSvcs { get; set; }

        public List<KernelAccessControlMmio> NormalMmio { get; set; }
        public List<KernelAccessControlMmio> PageMmio   { get; set; }
        public List<KernelAccessControlIrq>  Irq        { get; set; }

        public bool HasApplicationType { get; set; }
        public int  ApplicationType    { get; set; }

        public bool HasKernelVersion     { get; set; }
        public int  KernelVersionRelease { get; set; }

        public bool HasHandleTableSize { get; set; }
        public int  HandleTableSize    { get; set; }

        public bool HasDebugFlags { get; set; }
        public bool AllowDebug    { get; set; }
        public bool ForceDebug    { get; set; }
    }
}