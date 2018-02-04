using System;

namespace Gal
{
    public interface IGalRenderer
    {
        long FrameBufferPtr { get; set; }

        void QueueAction(Action ActionMthd);
        void RunActions();

        void Render();
        void SendVertexBuffer(int Index, byte[] Buffer, int Stride, GalVertexAttrib[] Attribs);
        void SendR8G8B8A8Texture(int Index, byte[] Buffer, int Width, int Height);
        void BindTexture(int Index);
    }
}