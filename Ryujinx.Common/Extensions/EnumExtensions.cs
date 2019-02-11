using System;

namespace Ryujinx.Common
{
    public static class EnumExtensions
    {
        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
