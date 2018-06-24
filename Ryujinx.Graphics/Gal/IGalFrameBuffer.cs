using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalFrameBuffer
    {
        void Create(long Key, int Width, int Height);

        void Bind(long Key);

        void BindTexture(long Key, int Index);

        void Set(long Key);

        void Set(byte[] Data, int Width, int Height);

        void SetTransform(float SX, float SY, float Rotate, float TX, float TY);

        void SetWindowSize(int Width, int Height);

        void SetViewport(int X, int Y, int Width, int Height);

        void Render();

        void GetBufferData(long Key, Action<byte[]> Callback);
    }
}