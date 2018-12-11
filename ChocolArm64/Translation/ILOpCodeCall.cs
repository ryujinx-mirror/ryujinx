using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeCall : IILEmit
    {
        private MethodInfo _mthdInfo;

        public ILOpCodeCall(MethodInfo mthdInfo)
        {
            _mthdInfo = mthdInfo;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(OpCodes.Call, _mthdInfo);
        }
    }
}