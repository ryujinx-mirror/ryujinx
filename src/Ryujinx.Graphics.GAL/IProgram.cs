using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IProgram : IDisposable
    {
        ProgramLinkStatus CheckProgramLink(bool blocking);

        byte[] GetBinary();
    }
}
