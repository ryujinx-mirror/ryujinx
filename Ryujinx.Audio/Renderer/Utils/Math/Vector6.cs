using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Utils.Math
{
    record struct Vector6
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        public float V;
        public float U;

        public Vector6(float value) : this(value, value, value, value, value, value)
        {
        }

        public Vector6(float x, float y, float z, float w, float v, float u)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            V = v;
            U = u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator +(Vector6 left, Vector6 right)
        {
            return new Vector6(left.X + right.X,
                               left.Y + right.Y,
                               left.Z + right.Z,
                               left.W + right.W,
                               left.V + right.V,
                               left.U + right.U);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(Vector6 left, Vector6 right)
        {
            return new Vector6(left.X * right.X,
                               left.Y * right.Y,
                               left.Z * right.Z,
                               left.W * right.W,
                               left.V * right.V,
                               left.U * right.U);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(Vector6 left, float right)
        {
            return left * new Vector6(right);
        }
    }
}
