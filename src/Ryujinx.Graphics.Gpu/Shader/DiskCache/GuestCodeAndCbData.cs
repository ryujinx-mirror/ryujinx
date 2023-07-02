namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Guest shader code and constant buffer data accessed by the shader.
    /// </summary>
    readonly struct GuestCodeAndCbData
    {
        /// <summary>
        /// Maxwell binary shader code.
        /// </summary>
        public byte[] Code { get; }

        /// <summary>
        /// Constant buffer 1 data accessed by the shader.
        /// </summary>
        public byte[] Cb1Data { get; }

        /// <summary>
        /// Creates a new instance of the guest shader code and constant buffer data.
        /// </summary>
        /// <param name="code">Maxwell binary shader code</param>
        /// <param name="cb1Data">Constant buffer 1 data accessed by the shader</param>
        public GuestCodeAndCbData(byte[] code, byte[] cb1Data)
        {
            Code = code;
            Cb1Data = cb1Data;
        }
    }
}
