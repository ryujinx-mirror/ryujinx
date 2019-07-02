using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using System.Collections.Generic;

namespace Ryujinx.Graphics
{
    public class GpuResourceManager
    {
        private enum ImageType
        {
            None,
            Texture,
            TextureArrayLayer,
            ColorBuffer,
            ZetaBuffer
        }

        private NvGpu _gpu;

        private HashSet<long>[] _uploadedKeys;

        private Dictionary<long, ImageType> _imageTypes;
        private Dictionary<long, int>      _mirroredTextures;

        public GpuResourceManager(NvGpu gpu)
        {
            _gpu = gpu;

            _uploadedKeys = new HashSet<long>[(int)NvGpuBufferType.Count];

            for (int index = 0; index < _uploadedKeys.Length; index++)
            {
                _uploadedKeys[index] = new HashSet<long>();
            }

            _imageTypes = new Dictionary<long, ImageType>();
            _mirroredTextures = new Dictionary<long, int>();
        }

        public void SendColorBuffer(NvGpuVmm vmm, long position, int attachment, GalImage newImage)
        {
            long size = (uint)ImageUtils.GetSize(newImage);

            _imageTypes[position] = ImageType.ColorBuffer;

            if (!TryReuse(vmm, position, newImage))
            {
                _gpu.Renderer.Texture.Create(position, (int)size, newImage);
            }

            _gpu.Renderer.RenderTarget.BindColor(position, attachment);
        }

        public void SendZetaBuffer(NvGpuVmm vmm, long position, GalImage newImage)
        {
            long size = (uint)ImageUtils.GetSize(newImage);

            _imageTypes[position] = ImageType.ZetaBuffer;

            if (!TryReuse(vmm, position, newImage))
            {
                _gpu.Renderer.Texture.Create(position, (int)size, newImage);
            }

            _gpu.Renderer.RenderTarget.BindZeta(position);
        }

        public void SendTexture(NvGpuVmm vmm, long position, GalImage newImage)
        {
            PrepareSendTexture(vmm, position, newImage);

            _imageTypes[position] = ImageType.Texture;
        }

        public bool TryGetTextureLayer(long position, out int layerIndex)
        {
            if (_mirroredTextures.TryGetValue(position, out layerIndex))
            {
                ImageType type = _imageTypes[position];

                // FIXME(thog): I'm actually unsure if we should deny all other image type, gpu testing needs to be done here.
                if (type != ImageType.Texture && type != ImageType.TextureArrayLayer)
                {
                    layerIndex = -1;
                    return false;
                }

                return true;
            }

            layerIndex = -1;
            return false;
        }

        public void SetTextureArrayLayer(long position, int layerIndex)
        {
            _imageTypes[position] = ImageType.TextureArrayLayer;
            _mirroredTextures[position] = layerIndex;
        }

        private void PrepareSendTexture(NvGpuVmm vmm, long position, GalImage newImage)
        {
            long size = ImageUtils.GetSize(newImage);

            bool skipCheck = false;

            if (_imageTypes.TryGetValue(position, out ImageType oldType))
            {
                if (oldType == ImageType.ColorBuffer || oldType == ImageType.ZetaBuffer)
                {
                    // Avoid data destruction
                    MemoryRegionModified(vmm, position, size, NvGpuBufferType.Texture);

                    skipCheck = true;
                }
            }

            if (skipCheck || !MemoryRegionModified(vmm, position, size, NvGpuBufferType.Texture))
            {
                if (TryReuse(vmm, position, newImage))
                {
                    return;
                }
            }

            byte[] data = ImageUtils.ReadTexture(vmm, newImage, position);

            _gpu.Renderer.Texture.Create(position, data, newImage);
        }

        private bool TryReuse(NvGpuVmm vmm, long position, GalImage newImage)
        {
            if (_gpu.Renderer.Texture.TryGetImage(position, out GalImage cachedImage) && cachedImage.TextureTarget == newImage.TextureTarget && cachedImage.SizeMatches(newImage))
            {
                _gpu.Renderer.RenderTarget.Reinterpret(position, newImage);

                return true;
            }

            return false;
        }

        public bool MemoryRegionModified(NvGpuVmm vmm, long position, long size, NvGpuBufferType type)
        {
            HashSet<long> uploaded = _uploadedKeys[(int)type];

            if (!uploaded.Add(position))
            {
                return false;
            }

            return vmm.IsRegionModified(position, size, type);
        }

        public void ClearPbCache()
        {
            for (int index = 0; index < _uploadedKeys.Length; index++)
            {
                _uploadedKeys[index].Clear();
            }
        }

        public void ClearPbCache(NvGpuBufferType type)
        {
            _uploadedKeys[(int)type].Clear();
        }
    }
}
