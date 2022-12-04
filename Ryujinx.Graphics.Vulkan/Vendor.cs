using System.Text.RegularExpressions;

namespace Ryujinx.Graphics.Vulkan
{
    enum Vendor
    {
        Amd,
        Intel,
        Nvidia,
        Qualcomm,
        Unknown
    }

    static partial class VendorUtils
    {
        [GeneratedRegex("Radeon (((HD|R(5|7|9|X)) )?((M?[2-6]\\d{2}(\\D|$))|([7-8]\\d{3}(\\D|$))|Fury|Nano))|(Pro Duo)")]
        public static partial Regex AmdGcnRegex();

        public static Vendor FromId(uint id)
        {
            return id switch
            {
                0x1002 => Vendor.Amd,
                0x10DE => Vendor.Nvidia,
                0x8086 => Vendor.Intel,
                0x5143 => Vendor.Qualcomm,
                _ => Vendor.Unknown
            };
        }

        public static string GetNameFromId(uint id)
        {
            return id switch
            {
                0x1002 => "AMD",
                0x1010 => "ImgTec",
                0x10DE => "NVIDIA",
                0x13B5 => "ARM",
                0x1AE0 => "Google",
                0x5143 => "Qualcomm",
                0x8086 => "Intel",
                0x10001 => "Vivante",
                0x10002 => "VeriSilicon",
                0x10003 => "Kazan",
                0x10004 => "Codeplay Software Ltd.",
                0x10005 => "Mesa",
                0x10006 => "PoCL",
                _ => $"0x{id:X}"
            };
        }
    }
}
