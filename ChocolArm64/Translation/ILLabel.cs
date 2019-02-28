using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class ILLabel : IILEmit
    {
        private bool _hasLabel;

        private Label _label;

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.MarkLabel(GetLabel(context));
        }

        public Label GetLabel(ILMethodBuilder context)
        {
            if (!_hasLabel)
            {
                _label = context.Generator.DefineLabel();

                _hasLabel = true;
            }

            return _label;
        }
    }
}