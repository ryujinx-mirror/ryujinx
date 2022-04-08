namespace Ryujinx.Audio.Renderer.Utils.Math
{
    record struct Matrix2x2
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;

        public Matrix2x2(float m11, float m12,
                         float m21, float m22)
        {
            M11 = m11;
            M12 = m12;

            M21 = m21;
            M22 = m22;
        }

        public static Matrix2x2 operator +(Matrix2x2 value1, Matrix2x2 value2)
        {
            Matrix2x2 m;

            m.M11 = value1.M11 + value2.M11;
            m.M12 = value1.M12 + value2.M12;
            m.M21 = value1.M21 + value2.M21;
            m.M22 = value1.M22 + value2.M22;

            return m;
        }

        public static Matrix2x2 operator -(Matrix2x2 value1, float value2)
        {
            Matrix2x2 m;

            m.M11 = value1.M11 - value2;
            m.M12 = value1.M12 - value2;
            m.M21 = value1.M21 - value2;
            m.M22 = value1.M22 - value2;

            return m;
        }

        public static Matrix2x2 operator *(Matrix2x2 value1, float value2)
        {
            Matrix2x2 m;

            m.M11 = value1.M11 * value2;
            m.M12 = value1.M12 * value2;
            m.M21 = value1.M21 * value2;
            m.M22 = value1.M22 * value2;

            return m;
        }

        public static Matrix2x2 operator *(Matrix2x2 value1, Matrix2x2 value2)
        {
            Matrix2x2 m;

            // First row
            m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21;
            m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22;

            // Second row
            m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21;
            m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22;

            return m;
        }
    }
}
