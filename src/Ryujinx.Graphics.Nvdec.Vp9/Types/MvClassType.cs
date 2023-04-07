namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal enum MvClassType
    {
        MvClass0 = 0,   /* (0, 2]     integer pel */
        MvClass1 = 1,   /* (2, 4]     integer pel */
        MvClass2 = 2,   /* (4, 8]     integer pel */
        MvClass3 = 3,   /* (8, 16]    integer pel */
        MvClass4 = 4,   /* (16, 32]   integer pel */
        MvClass5 = 5,   /* (32, 64]   integer pel */
        MvClass6 = 6,   /* (64, 128]  integer pel */
        MvClass7 = 7,   /* (128, 256] integer pel */
        MvClass8 = 8,   /* (256, 512] integer pel */
        MvClass9 = 9,   /* (512, 1024] integer pel */
        MvClass10 = 10, /* (1024,2048] integer pel */
    }
}
