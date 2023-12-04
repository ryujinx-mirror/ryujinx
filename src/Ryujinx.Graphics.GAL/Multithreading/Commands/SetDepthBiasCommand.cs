namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetDepthBiasCommand : IGALCommand, IGALCommand<SetDepthBiasCommand>
    {
        public readonly CommandType CommandType => CommandType.SetDepthBias;
        private PolygonModeMask _enables;
        private float _factor;
        private float _units;
        private float _clamp;

        public void Set(PolygonModeMask enables, float factor, float units, float clamp)
        {
            _enables = enables;
            _factor = factor;
            _units = units;
            _clamp = clamp;
        }

        public static void Run(ref SetDepthBiasCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetDepthBias(command._enables, command._factor, command._units, command._clamp);
        }
    }
}
