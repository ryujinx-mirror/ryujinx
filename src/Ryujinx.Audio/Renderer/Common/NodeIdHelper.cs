namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Helper for manipulating node ids.
    /// </summary>
    public static class NodeIdHelper
    {
        /// <summary>
        /// Get the type of a node from a given node id.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        /// <returns>The type of the node.</returns>
        public static NodeIdType GetType(int nodeId)
        {
            return (NodeIdType)(nodeId >> 28);
        }

        /// <summary>
        /// Get the base of a node from a given node id.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        /// <returns>The base of the node.</returns>
        public static int GetBase(int nodeId)
        {
            return (nodeId >> 16) & 0xFFF;
        }
    }
}
