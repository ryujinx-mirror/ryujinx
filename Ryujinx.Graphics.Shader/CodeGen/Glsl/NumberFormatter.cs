using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Globalization;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class NumberFormatter
    {
        private const int MaxDecimal = 256;

        public static bool TryFormat(int value, VariableType dstType, out string formatted)
        {
            if (dstType == VariableType.F32 || dstType == VariableType.F64)
            {
                return TryFormatFloat(BitConverter.Int32BitsToSingle(value), out formatted);
            }
            else if (dstType == VariableType.S32)
            {
                formatted = FormatInt(value);
            }
            else if (dstType == VariableType.U32)
            {
                formatted = FormatUint((uint)value);
            }
            else if (dstType == VariableType.Bool)
            {
                formatted = value != 0 ? "true" : "false";
            }
            else
            {
                throw new ArgumentException($"Invalid variable type \"{dstType}\".");
            }

            return true;
        }

        public static string FormatFloat(float value)
        {
            if (!TryFormatFloat(value, out string formatted))
            {
                throw new ArgumentException("Failed to convert float value to string.");
            }

            return formatted;
        }

        public static bool TryFormatFloat(float value, out string formatted)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                formatted = null;

                return false;
            }

            formatted = value.ToString("G9", CultureInfo.InvariantCulture);

            if (!(formatted.Contains('.') ||
                  formatted.Contains('e') ||
                  formatted.Contains('E')))
            {
                formatted += ".0";
            }

            return true;
        }

        public static string FormatInt(int value, VariableType dstType)
        {
            if (dstType == VariableType.S32)
            {
                return FormatInt(value);
            }
            else if (dstType == VariableType.U32)
            {
                return FormatUint((uint)value);
            }
            else
            {
                throw new ArgumentException($"Invalid variable type \"{dstType}\".");
            }
        }

        public static string FormatInt(int value)
        {
            if (value <= MaxDecimal && value >= -MaxDecimal)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return "0x" + value.ToString("X", CultureInfo.InvariantCulture);
        }

        public static string FormatUint(uint value)
        {
            if (value <= MaxDecimal && value >= 0)
            {
                return value.ToString(CultureInfo.InvariantCulture) + "u";
            }

            return "0x" + value.ToString("X", CultureInfo.InvariantCulture) + "u";
        }
    }
}