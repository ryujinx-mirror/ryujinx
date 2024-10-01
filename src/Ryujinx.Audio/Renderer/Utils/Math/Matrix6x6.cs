namespace Ryujinx.Audio.Renderer.Utils.Math
{
    record struct Matrix6x6
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M15;
        public float M16;

        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M25;
        public float M26;

        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M35;
        public float M36;

        public float M41;
        public float M42;
        public float M43;
        public float M44;
        public float M45;
        public float M46;

        public float M51;
        public float M52;
        public float M53;
        public float M54;
        public float M55;
        public float M56;

        public float M61;
        public float M62;
        public float M63;
        public float M64;
        public float M65;
        public float M66;

        public Matrix6x6(float m11, float m12, float m13, float m14, float m15, float m16,
                         float m21, float m22, float m23, float m24, float m25, float m26,
                         float m31, float m32, float m33, float m34, float m35, float m36,
                         float m41, float m42, float m43, float m44, float m45, float m46,
                         float m51, float m52, float m53, float m54, float m55, float m56,
                         float m61, float m62, float m63, float m64, float m65, float m66)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M15 = m15;
            M16 = m16;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M25 = m25;
            M26 = m26;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M35 = m35;
            M36 = m36;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
            M45 = m45;
            M46 = m46;

            M51 = m51;
            M52 = m52;
            M53 = m53;
            M54 = m54;
            M55 = m55;
            M56 = m56;

            M61 = m61;
            M62 = m62;
            M63 = m63;
            M64 = m64;
            M65 = m65;
            M66 = m66;
        }
    }
}
