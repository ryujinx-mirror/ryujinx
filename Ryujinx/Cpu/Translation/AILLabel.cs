using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class AILLabel : IAILEmit
    {
        private bool HasLabel;

        private Label Lbl;

        public void Emit(AILEmitter Context)
        {
            Context.Generator.MarkLabel(GetLabel(Context));
        }

        public Label GetLabel(AILEmitter Context)
        {
            if (!HasLabel)
            {
                Lbl = Context.Generator.DefineLabel();

                HasLabel = true;
            }

            return Lbl;
        }
    }
}