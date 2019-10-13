namespace Ryujinx.Graphics.GAL
{
    public interface IComputePipeline
    {
        void Dispatch(int groupsX, int groupsY, int groupsZ);

        void SetProgram(IProgram program);

        void SetStorageBuffer(int index, BufferRange buffer);
        void SetUniformBuffer(int index, BufferRange buffer);
    }
}
