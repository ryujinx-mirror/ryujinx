namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal enum MvJointType
    {
        MvJointZero = 0,   /* Zero vector */
        MvJointHnzvz = 1,  /* Vert zero, hor nonzero */
        MvJointHzvnz = 2,  /* Hor zero, vert nonzero */
        MvJointHnzvnz = 3, /* Both components nonzero */
    }
}
