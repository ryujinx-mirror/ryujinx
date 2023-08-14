namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenFSI
    {
        public static string FSIBegin(CodeGenContext context)
        {
            if (context.HostCapabilities.SupportsFragmentShaderInterlock)
            {
                return "beginInvocationInterlockARB()";
            }
            else if (context.HostCapabilities.SupportsFragmentShaderOrderingIntel)
            {
                return "beginFragmentShaderOrderingINTEL()";
            }

            return null;
        }

        public static string FSIEnd(CodeGenContext context)
        {
            if (context.HostCapabilities.SupportsFragmentShaderInterlock)
            {
                return "endInvocationInterlockARB()";
            }

            return null;
        }
    }
}
