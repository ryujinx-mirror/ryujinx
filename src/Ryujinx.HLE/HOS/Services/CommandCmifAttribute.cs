using System;

namespace Ryujinx.HLE.HOS.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class CommandCmifAttribute : Attribute
    {
        public readonly int Id;

        public CommandCmifAttribute(int id) => Id = id;
    }
}
