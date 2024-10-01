using Ryujinx.Audio.Renderer.Parameter;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    struct AudioRendererParameterInternal
    {
        public AudioRendererConfiguration Configuration;

        public AudioRendererParameterInternal(AudioRendererConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
