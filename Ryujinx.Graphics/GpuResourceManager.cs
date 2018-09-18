using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System.Collections.Generic;

namespace Ryujinx.Graphics
{
    public class GpuResourceManager
    {
        private NvGpu Gpu;

        private HashSet<long>[] UploadedKeys;

        public GpuResourceManager(NvGpu Gpu)
        {
            this.Gpu = Gpu;

            UploadedKeys = new HashSet<long>[(int)NvGpuBufferType.Count];

            for (int Index = 0; Index < UploadedKeys.Length; Index++)
            {
                UploadedKeys[Index] = new HashSet<long>();
            }
        }

        public void SendColorBuffer(NvGpuVmm Vmm, long Position, int Attachment, GalImage NewImage)
        {
            long Size = (uint)ImageUtils.GetSize(NewImage);

            MarkAsCached(Vmm, Position, Size, NvGpuBufferType.Texture);

            bool IsCached = Gpu.Renderer.Texture.TryGetImage(Position, out GalImage CachedImage);

            if (IsCached && CachedImage.SizeMatches(NewImage))
            {
                Gpu.Renderer.RenderTarget.Reinterpret(Position, NewImage);
                Gpu.Renderer.RenderTarget.BindColor(Position, Attachment, NewImage);

                return;
            }

            Gpu.Renderer.Texture.Create(Position, (int)Size, NewImage);

            Gpu.Renderer.RenderTarget.BindColor(Position, Attachment, NewImage);
        }

        public void SendZetaBuffer(NvGpuVmm Vmm, long Position, GalImage NewImage)
        {
            long Size = (uint)ImageUtils.GetSize(NewImage);

            MarkAsCached(Vmm, Position, Size, NvGpuBufferType.Texture);

            bool IsCached = Gpu.Renderer.Texture.TryGetImage(Position, out GalImage CachedImage);

            if (IsCached && CachedImage.SizeMatches(NewImage))
            {
                Gpu.Renderer.RenderTarget.Reinterpret(Position, NewImage);
                Gpu.Renderer.RenderTarget.BindZeta(Position, NewImage);

                return;
            }

            Gpu.Renderer.Texture.Create(Position, (int)Size, NewImage);

            Gpu.Renderer.RenderTarget.BindZeta(Position, NewImage);
        }

        public void SendTexture(NvGpuVmm Vmm, long Position, GalImage NewImage, int TexIndex = -1)
        {
            long Size = (uint)ImageUtils.GetSize(NewImage);

            if (!MemoryRegionModified(Vmm, Position, Size, NvGpuBufferType.Texture))
            {
                if (Gpu.Renderer.Texture.TryGetImage(Position, out GalImage CachedImage) && CachedImage.SizeMatches(NewImage))
                {
                    Gpu.Renderer.RenderTarget.Reinterpret(Position, NewImage);

                    if (TexIndex >= 0)
                    {
                        Gpu.Renderer.Texture.Bind(Position, TexIndex, NewImage);
                    }

                    return;
                }
            }

            byte[] Data = ImageUtils.ReadTexture(Vmm, NewImage, Position);

            Gpu.Renderer.Texture.Create(Position, Data, NewImage);

            if (TexIndex >= 0)
            {
                Gpu.Renderer.Texture.Bind(Position, TexIndex, NewImage);
            }
        }

        private void MarkAsCached(NvGpuVmm Vmm, long Position, long Size, NvGpuBufferType Type)
        {
            Vmm.IsRegionModified(Position, Size, Type);
        }

        private bool MemoryRegionModified(NvGpuVmm Vmm, long Position, long Size, NvGpuBufferType Type)
        {
            HashSet<long> Uploaded = UploadedKeys[(int)Type];

            if (!Uploaded.Add(Position))
            {
                return false;
            }

            return Vmm.IsRegionModified(Position, Size, Type);
        }

        public void ClearPbCache()
        {
            for (int Index = 0; Index < UploadedKeys.Length; Index++)
            {
                UploadedKeys[Index].Clear();
            }
        }
    }
}
