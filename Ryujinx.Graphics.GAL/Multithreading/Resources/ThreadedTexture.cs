using Ryujinx.Common.Memory;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Texture;
using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Resources
{
    /// <summary>
    /// Threaded representation of a texture.
    /// </summary>
    class ThreadedTexture : ITexture
    {
        private ThreadedRenderer _renderer;
        private TextureCreateInfo _info;
        public ITexture Base;

        public int Width => _info.Width;

        public int Height => _info.Height;

        public float ScaleFactor { get; }

        public ThreadedTexture(ThreadedRenderer renderer, TextureCreateInfo info, float scale)
        {
            _renderer = renderer;
            _info = info;
            ScaleFactor = scale;
        }

        private TableRef<T> Ref<T>(T reference)
        {
            return new TableRef<T>(_renderer, reference);
        }

        public void CopyTo(ITexture destination, int firstLayer, int firstLevel)
        {
            _renderer.New<TextureCopyToCommand>().Set(Ref(this), Ref((ThreadedTexture)destination), firstLayer, firstLevel);
            _renderer.QueueCommand();
        }

        public void CopyTo(ITexture destination, int srcLayer, int dstLayer, int srcLevel, int dstLevel)
        {
            _renderer.New<TextureCopyToSliceCommand>().Set(Ref(this), Ref((ThreadedTexture)destination), srcLayer, dstLayer, srcLevel, dstLevel);
            _renderer.QueueCommand();
        }

        public void CopyTo(ITexture destination, Extents2D srcRegion, Extents2D dstRegion, bool linearFilter)
        {
            ThreadedTexture dest = (ThreadedTexture)destination;

            if (_renderer.IsGpuThread())
            {
                _renderer.New<TextureCopyToScaledCommand>().Set(Ref(this), Ref(dest), srcRegion, dstRegion, linearFilter);
                _renderer.QueueCommand();
            }
            else
            {
                // Scaled copy can happen on another thread for a res scale flush.
                ThreadedHelpers.SpinUntilNonNull(ref Base);
                ThreadedHelpers.SpinUntilNonNull(ref dest.Base);

                Base.CopyTo(dest.Base, srcRegion, dstRegion, linearFilter);
            }
        }

        public ITexture CreateView(TextureCreateInfo info, int firstLayer, int firstLevel)
        {
            ThreadedTexture newTex = new ThreadedTexture(_renderer, info, ScaleFactor);
            _renderer.New<TextureCreateViewCommand>().Set(Ref(this), Ref(newTex), info, firstLayer, firstLevel);
            _renderer.QueueCommand();

            return newTex;
        }

        public PinnedSpan<byte> GetData()
        {
            if (_renderer.IsGpuThread())
            {
                ResultBox<PinnedSpan<byte>> box = new ResultBox<PinnedSpan<byte>>();
                _renderer.New<TextureGetDataCommand>().Set(Ref(this), Ref(box));
                _renderer.InvokeCommand();

                return box.Result;
            }
            else
            {
                ThreadedHelpers.SpinUntilNonNull(ref Base);

                return Base.GetData();
            }
        }

        public PinnedSpan<byte> GetData(int layer, int level)
        {
            if (_renderer.IsGpuThread())
            {
                ResultBox<PinnedSpan<byte>> box = new ResultBox<PinnedSpan<byte>>();
                _renderer.New<TextureGetDataSliceCommand>().Set(Ref(this), Ref(box), layer, level);
                _renderer.InvokeCommand();

                return box.Result;
            }
            else
            {
                ThreadedHelpers.SpinUntilNonNull(ref Base);

                return Base.GetData(layer, level);
            }
        }

        public void SetData(SpanOrArray<byte> data)
        {
            _renderer.New<TextureSetDataCommand>().Set(Ref(this), Ref(data.ToArray()));
            _renderer.QueueCommand();
        }

        public void SetData(SpanOrArray<byte> data, int layer, int level)
        {
            _renderer.New<TextureSetDataSliceCommand>().Set(Ref(this), Ref(data.ToArray()), layer, level);
            _renderer.QueueCommand();
        }

        public void SetData(SpanOrArray<byte> data, int layer, int level, Rectangle<int> region)
        {
            _renderer.New<TextureSetDataSliceRegionCommand>().Set(Ref(this), Ref(data.ToArray()), layer, level, region);
            _renderer.QueueCommand();
        }

        public void SetStorage(BufferRange buffer)
        {
            _renderer.New<TextureSetStorageCommand>().Set(Ref(this), buffer);
            _renderer.QueueCommand();
        }

        public void Release()
        {
            _renderer.New<TextureReleaseCommand>().Set(Ref(this));
            _renderer.QueueCommand();
        }
    }
}
