namespace Ryujinx.Graphics.Gpu.Synchronization
{
    /// <summary>
    /// This interface indicates that a class can be registered for a sync action.
    /// </summary>
    interface ISyncActionHandler
    {
        /// <summary>
        /// Action to be performed when some synchronizing action is reached after modification.
        /// Generally used to register read/write tracking to flush resources from GPU when their memory is used.
        /// </summary>
        /// <param name="syncpoint">True if the action is a guest syncpoint</param>
        /// <returns>True if the action is to be removed, false otherwise</returns>
        bool SyncAction(bool syncpoint);

        /// <summary>
        /// Action to be performed immediately before sync is created.
        /// </summary>
        /// <param name="syncpoint">True if the action is a guest syncpoint</param>
        void SyncPreAction(bool syncpoint) { }
    }
}
