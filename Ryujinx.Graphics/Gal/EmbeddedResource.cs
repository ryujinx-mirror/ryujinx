using System.IO;
using System.Reflection;

namespace Ryujinx.Graphics.Gal
{
    static class EmbeddedResource
    {
        public static string GetString(string Name)
        {
            Assembly Asm = typeof(EmbeddedResource).Assembly;

            using (Stream ResStream = Asm.GetManifestResourceStream(Name))
            {
                StreamReader Reader = new StreamReader(ResStream);

                return Reader.ReadToEnd();
            }
        }
    }
}