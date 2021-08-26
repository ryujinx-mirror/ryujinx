using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetSamplerCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetSampler;
        private int _index;
        private TableRef<ISampler> _sampler;

        public void Set(int index, TableRef<ISampler> sampler)
        {
            _index = index;
            _sampler = sampler;
        }

        public static void Run(ref SetSamplerCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetSampler(command._index, command._sampler.GetAs<ThreadedSampler>(threaded)?.Base);
        }
    }
}
