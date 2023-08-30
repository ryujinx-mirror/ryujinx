using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class ShaderAsCompute
    {
        public IProgram HostProgram { get; }
        public ShaderProgramInfo Info { get; }
        public ResourceReservations Reservations { get; }

        public ShaderAsCompute(IProgram hostProgram, ShaderProgramInfo info, ResourceReservations reservations)
        {
            HostProgram = hostProgram;
            Info = info;
            Reservations = reservations;
        }
    }
}
