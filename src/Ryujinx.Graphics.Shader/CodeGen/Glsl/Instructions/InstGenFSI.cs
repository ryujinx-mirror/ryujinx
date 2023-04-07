namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenFSI
    {
        public static string FSIBegin(CodeGenContext context)
        {
            if (context.Config.GpuAccessor.QueryHostSupportsFragmentShaderInterlock())
            {
                return "beginInvocationInterlockARB()";
            }
            else if (context.Config.GpuAccessor.QueryHostSupportsFragmentShaderOrderingIntel())
            {
                return "beginFragmentShaderOrderingINTEL()";
            }

            return null;
        }

        public static string FSIEnd(CodeGenContext context)
        {
            if (context.Config.GpuAccessor.QueryHostSupportsFragmentShaderInterlock())
            {
                return "endInvocationInterlockARB()";
            }

            return null;
        }
    }
}