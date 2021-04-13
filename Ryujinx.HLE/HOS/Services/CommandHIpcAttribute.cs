using System;

namespace Ryujinx.HLE.HOS.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class CommandHipcAttribute : Attribute
    {
        public readonly int Id;

        public CommandHipcAttribute(int id) => Id = id;
    }
}