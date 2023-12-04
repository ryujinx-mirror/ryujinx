namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// One side of a two-way dependency between one texture view and another.
    /// Contains a reference to the handle owning the dependency, and the other dependency.
    /// </summary>
    class TextureDependency
    {
        /// <summary>
        /// The handle that owns this dependency.
        /// </summary>
        public TextureGroupHandle Handle;

        /// <summary>
        /// The other dependency linked to this one, which belongs to another handle.
        /// </summary>
        public TextureDependency Other;

        /// <summary>
        /// Create a new texture dependency.
        /// </summary>
        /// <param name="handle">The handle that owns the dependency</param>
        public TextureDependency(TextureGroupHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Signal that the owner of this dependency has been modified,
        /// meaning that the other dependency's handle must defer a copy from it.
        /// </summary>
        public void SignalModified()
        {
            Other.Handle.DeferCopy(Handle);
        }
    }
}
