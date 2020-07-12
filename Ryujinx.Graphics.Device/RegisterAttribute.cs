using System;

namespace Ryujinx.Graphics.Device
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RegisterAttribute : Attribute
    {
        public AccessControl AccessControl { get; }

        public RegisterAttribute(AccessControl ac)
        {
            AccessControl = ac;
        }
    }
}
