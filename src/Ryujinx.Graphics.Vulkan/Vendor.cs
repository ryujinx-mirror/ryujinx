using Silk.NET.Vulkan;
using System.Text.RegularExpressions;

namespace Ryujinx.Graphics.Vulkan
{
    enum Vendor
    {
        Amd,
        ImgTec,
        Intel,
        Nvidia,
        ARM,
        Broadcom,
        Qualcomm,
        Apple,
        Unknown,
    }

    static partial class VendorUtils
    {
        [GeneratedRegex("Radeon (((HD|R(5|7|9|X)) )?((M?[2-6]\\d{2}(\\D|$))|([7-8]\\d{3}(\\D|$))|Fury|Nano))|(Pro Duo)")]
        public static partial Regex AmdGcnRegex();

        [GeneratedRegex("NVIDIA GeForce (R|G)?TX? (\\d{3}\\d?)M?")]
        public static partial Regex NvidiaConsumerClassRegex();

        public static Vendor FromId(uint id)
        {
            return id switch
            {
                0x1002 => Vendor.Amd,
                0x1010 => Vendor.ImgTec,
                0x106B => Vendor.Apple,
                0x10DE => Vendor.Nvidia,
                0x13B5 => Vendor.ARM,
                0x14E4 => Vendor.Broadcom,
                0x8086 => Vendor.Intel,
                0x5143 => Vendor.Qualcomm,
                _ => Vendor.Unknown,
            };
        }

        public static string GetNameFromId(uint id)
        {
            return id switch
            {
                0x1002 => "AMD",
                0x1010 => "ImgTec",
                0x106B => "Apple",
                0x10DE => "NVIDIA",
                0x13B5 => "ARM",
                0x14E4 => "Broadcom",
                0x1AE0 => "Google",
                0x5143 => "Qualcomm",
                0x8086 => "Intel",
                0x10001 => "Vivante",
                0x10002 => "VeriSilicon",
                0x10003 => "Kazan",
                0x10004 => "Codeplay Software Ltd.",
                0x10005 => "Mesa",
                0x10006 => "PoCL",
                _ => $"0x{id:X}",
            };
        }

        public static string GetFriendlyDriverName(DriverId id)
        {
            return id switch
            {
                DriverId.AmdProprietary => "AMD",
                DriverId.AmdOpenSource => "AMD (Open)",
                DriverId.ArmProprietary => "ARM",
                DriverId.BroadcomProprietary => "Broadcom",
                DriverId.CoreaviProprietary => "CoreAVI",
                DriverId.GgpProprietary => "GGP",
                DriverId.GoogleSwiftshader => "SwiftShader",
                DriverId.ImaginationProprietary => "Imagination",
                DriverId.IntelOpenSourceMesa => "Intel (Open)",
                DriverId.IntelProprietaryWindows => "Intel",
                DriverId.JuiceProprietary => "Juice",
                DriverId.MesaDozen => "Dozen",
                DriverId.MesaLlvmpipe => "LLVMpipe",
                DriverId.MesaPanvk => "PanVK",
                DriverId.MesaRadv => "RADV",
                DriverId.MesaTurnip => "Turnip",
                DriverId.MesaV3DV => "V3DV",
                DriverId.MesaVenus => "Venus",
                DriverId.Moltenvk => "MoltenVK",
                DriverId.NvidiaProprietary => "NVIDIA",
                DriverId.QualcommProprietary => "Qualcomm",
                DriverId.SamsungProprietary => "Samsung",
                DriverId.VerisiliconProprietary => "Verisilicon",
                _ => id.ToString(),
            };
        }
    }
}
