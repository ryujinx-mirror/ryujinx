using System.IO;
using System.Reflection;

namespace Ryujinx.Graphics.Gal
{
    static class EmbeddedResource
    {
        public static string GetString(string name)
        {
            Assembly asm = typeof(EmbeddedResource).Assembly;

            using (Stream resStream = asm.GetManifestResourceStream(name))
            {
                StreamReader reader = new StreamReader(resStream);

                return reader.ReadToEnd();
            }
        }
    }
}