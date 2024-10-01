using System;

namespace ARMeilleure.Common
{
    static class EnumUtils
    {
        public static int GetCount(Type enumType)
        {
            return Enum.GetNames(enumType).Length;
        }
    }
}
