using System.Collections.Concurrent;

namespace Ryujinx.Core.OsHle.Services.Nv
{
    class NvChNvMap
    {
        private static ConcurrentDictionary<Process, IdDictionary> NvMaps;

        public void Create(ServiceCtx Context)
        {
            long InputPosition  = Context.Request.GetBufferType0x21Position();
            long OutputPosition = Context.Request.GetBufferType0x22Position();

            int Size = Context.Memory.ReadInt32(InputPosition);

            int Handle = AddNvMap(Context, new NvMap(Size));

            Context.Memory.WriteInt32(OutputPosition, Handle);
        }

        private int AddNvMap(ServiceCtx Context, NvMap Map)
        {
            return NvMaps[Context.Process].Add(Map);
        }

        public NvMap GetNvMap(ServiceCtx Context, int Handle)
        {
            return NvMaps[Context.Process].GetData<NvMap>(Handle);
        }
    }
}