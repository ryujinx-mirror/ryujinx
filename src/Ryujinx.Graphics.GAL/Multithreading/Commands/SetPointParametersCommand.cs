namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetPointParametersCommand : IGALCommand, IGALCommand<SetPointParametersCommand>
    {
        public readonly CommandType CommandType => CommandType.SetPointParameters;
        private float _size;
        private bool _isProgramPointSize;
        private bool _enablePointSprite;
        private Origin _origin;

        public void Set(float size, bool isProgramPointSize, bool enablePointSprite, Origin origin)
        {
            _size = size;
            _isProgramPointSize = isProgramPointSize;
            _enablePointSprite = enablePointSprite;
            _origin = origin;
        }

        public static void Run(ref SetPointParametersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetPointParameters(command._size, command._isProgramPointSize, command._enablePointSprite, command._origin);
        }
    }
}
