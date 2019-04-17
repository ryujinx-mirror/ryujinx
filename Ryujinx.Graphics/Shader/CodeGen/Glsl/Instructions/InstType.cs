using System;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    [Flags]
    enum InstType
    {
        OpNullary   = Op | 0,
        OpUnary     = Op | 1,
        OpBinary    = Op | 2,
        OpTernary   = Op | 3,
        OpBinaryCom = OpBinary | Comutative,

        CallNullary    = Call | 0,
        CallUnary      = Call | 1,
        CallBinary     = Call | 2,
        CallTernary    = Call | 3,
        CallQuaternary = Call | 4,

        Comutative = 1 << 8,
        Op         = 1 << 9,
        Call       = 1 << 10,
        Special    = 1 << 11,

        ArityMask = 0xff
    }
}