using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeCall : IAILEmit
    {
        private MethodInfo MthdInfo;

        public AILOpCodeCall(MethodInfo MthdInfo)
        {
            this.MthdInfo = MthdInfo;
        }

        public void Emit(AILEmitter Context)
        {
            Context.Generator.Emit(OpCodes.Call, MthdInfo);
        }
    }
}