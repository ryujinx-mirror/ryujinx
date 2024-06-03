using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.ImageArray
{
    struct ImageArrayDisposeCommand : IGALCommand, IGALCommand<ImageArrayDisposeCommand>
    {
        public readonly CommandType CommandType => CommandType.ImageArrayDispose;
        private TableRef<ThreadedImageArray> _imageArray;

        public void Set(TableRef<ThreadedImageArray> imageArray)
        {
            _imageArray = imageArray;
        }

        public static void Run(ref ImageArrayDisposeCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._imageArray.Get(threaded).Base.Dispose();
        }
    }
}
