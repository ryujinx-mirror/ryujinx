using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class ILLabel : IILEmit
    {
        private bool _hasLabel;

        private Label _lbl;

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.MarkLabel(GetLabel(context));
        }

        public Label GetLabel(ILMethodBuilder context)
        {
            if (!_hasLabel)
            {
                _lbl = context.Generator.DefineLabel();

                _hasLabel = true;
            }

            return _lbl;
        }
    }
}