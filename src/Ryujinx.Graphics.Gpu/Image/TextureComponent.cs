using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture swizzle color component.
    /// </summary>
    enum TextureComponent
    {
        Zero = 0,
        Red = 2,
        Green = 3,
        Blue = 4,
        Alpha = 5,
        OneSI = 6,
        OneF = 7,
    }

    static class TextureComponentConverter
    {
        /// <summary>
        /// Converts the texture swizzle color component enum to the respective Graphics Abstraction Layer enum.
        /// </summary>
        /// <param name="component">Texture swizzle color component</param>
        /// <returns>Converted enum</returns>
        public static SwizzleComponent Convert(this TextureComponent component)
        {
            return component switch
            {
                TextureComponent.Zero => SwizzleComponent.Zero,
                TextureComponent.Red => SwizzleComponent.Red,
                TextureComponent.Green => SwizzleComponent.Green,
                TextureComponent.Blue => SwizzleComponent.Blue,
                TextureComponent.Alpha => SwizzleComponent.Alpha,
                TextureComponent.OneSI or TextureComponent.OneF => SwizzleComponent.One,
                _ => SwizzleComponent.Zero,
            };
        }
    }
}
