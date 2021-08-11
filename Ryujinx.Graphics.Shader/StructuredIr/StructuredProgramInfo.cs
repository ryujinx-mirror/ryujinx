using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class StructuredProgramInfo
    {
        public List<StructuredFunction> Functions { get; }

        public HelperFunctionsMask HelperFunctionsMask { get; set; }

        public StructuredProgramInfo()
        {
            Functions = new List<StructuredFunction>();
        }
    }
}