using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Vulkan
{
    readonly struct ResourceBindingSegment
    {
        public readonly int Binding;
        public readonly int Count;
        public readonly ResourceType Type;
        public readonly ResourceStages Stages;

        public ResourceBindingSegment(int binding, int count, ResourceType type, ResourceStages stages)
        {
            Binding = binding;
            Count = count;
            Type = type;
            Stages = stages;
        }
    }
}
