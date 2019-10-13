using System;

namespace Ryujinx.Graphics.Gpu
{
    static class Debugging
    {
        public static void PrintTexInfo(string prefix, Image.Texture tex)
        {
            if (tex == null)
            {
                Console.WriteLine(prefix + " null");

                return;
            }

            string range = $"{tex.Address:X}..{(tex.Address + tex.Size):X}";

            int debugId = tex.HostTexture.GetStorageDebugId();

            string str = $"{prefix} p {debugId:X8} {tex.Info.Target} {tex.Info.FormatInfo.Format} {tex.Info.Width}x{tex.Info.Height}x{tex.Info.DepthOrLayers} mips {tex.Info.Levels} addr {range}";

            Console.WriteLine(str);
        }
    }
}