using Ryujinx.Audio.Renderer.Utils.Math;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp
{
    static class MatrixHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 Transform(ref Vector6 value1, ref Matrix6x6 value2)
        {
            return new Vector6
            {
                X = value2.M11 * value1.X + value2.M12 * value1.Y + value2.M13 * value1.Z + value2.M14 * value1.W + value2.M15 * value1.V + value2.M16 * value1.U,
                Y = value2.M21 * value1.X + value2.M22 * value1.Y + value2.M23 * value1.Z + value2.M24 * value1.W + value2.M25 * value1.V + value2.M26 * value1.U,
                Z = value2.M31 * value1.X + value2.M32 * value1.Y + value2.M33 * value1.Z + value2.M34 * value1.W + value2.M35 * value1.V + value2.M36 * value1.U,
                W = value2.M41 * value1.X + value2.M42 * value1.Y + value2.M43 * value1.Z + value2.M44 * value1.W + value2.M45 * value1.V + value2.M46 * value1.U,
                V = value2.M51 * value1.X + value2.M52 * value1.Y + value2.M53 * value1.Z + value2.M54 * value1.W + value2.M55 * value1.V + value2.M56 * value1.U,
                U = value2.M61 * value1.X + value2.M62 * value1.Y + value2.M63 * value1.Z + value2.M64 * value1.W + value2.M65 * value1.V + value2.M66 * value1.U,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Transform(ref Vector4 value1, ref Matrix4x4 value2)
        {
            return new Vector4
            {
                X = value2.M11 * value1.X + value2.M12 * value1.Y + value2.M13 * value1.Z + value2.M14 * value1.W,
                Y = value2.M21 * value1.X + value2.M22 * value1.Y + value2.M23 * value1.Z + value2.M24 * value1.W,
                Z = value2.M31 * value1.X + value2.M32 * value1.Y + value2.M33 * value1.Z + value2.M34 * value1.W,
                W = value2.M41 * value1.X + value2.M42 * value1.Y + value2.M43 * value1.Z + value2.M44 * value1.W
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(ref Vector2 value1, ref Matrix2x2 value2)
        {
            return new Vector2
            {
                X = value2.M11 * value1.X + value2.M12 * value1.Y,
                Y = value2.M21 * value1.X + value2.M22 * value1.Y,
            };
        }
    }
}
