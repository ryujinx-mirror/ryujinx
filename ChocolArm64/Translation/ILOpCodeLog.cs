namespace ChocolArm64.Translation
{
    struct ILOpCodeLog : IILEmit
    {
        public string Text { get; }

        public ILOpCodeLog(string text)
        {
            Text = text;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.EmitWriteLine(Text);
        }
    }
}