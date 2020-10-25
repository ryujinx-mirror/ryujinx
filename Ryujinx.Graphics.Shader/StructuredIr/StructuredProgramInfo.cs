using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class StructuredProgramInfo
    {
        public List<StructuredFunction> Functions { get; }

        public HashSet<int> CBuffers { get; }
        public HashSet<int> SBuffers { get; }

        public HashSet<int> IAttributes { get; }
        public HashSet<int> OAttributes { get; }

        public bool UsesInstanceId { get; set; }
        public bool UsesCbIndexing { get; set; }

        public HelperFunctionsMask HelperFunctionsMask { get; set; }

        public HashSet<AstTextureOperation> Samplers { get; }
        public HashSet<AstTextureOperation> Images   { get; }

        public StructuredProgramInfo()
        {
            Functions = new List<StructuredFunction>();

            CBuffers = new HashSet<int>();
            SBuffers = new HashSet<int>();

            IAttributes = new HashSet<int>();
            OAttributes = new HashSet<int>();

            Samplers = new HashSet<AstTextureOperation>();
            Images   = new HashSet<AstTextureOperation>();
        }
    }
}