using System;

namespace Ryujinx.Graphics.Gal
{
    public unsafe interface IGalRenderer
    {
        void QueueAction(Action ActionMthd);
        void RunActions();

        void InitializeFrameBuffer();
        void Render();
        void SetWindowSize(int Width, int Height);
        void SetFrameBuffer(byte* Fb, int Width, int Height, float SX, float SY, float R);
        void SendVertexBuffer(int Index, byte[] Buffer, int Stride, GalVertexAttrib[] Attribs);
        void SendR8G8B8A8Texture(int Index, byte[] Buffer, int Width, int Height);
        void BindTexture(int Index);
    }
}