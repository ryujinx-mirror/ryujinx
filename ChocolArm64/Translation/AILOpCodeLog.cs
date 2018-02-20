namespace ChocolArm64.Translation
{
    struct AILOpCodeLog : IAILEmit
    {
        private string Text;

        public AILOpCodeLog(string Text)
        {
            this.Text = Text;
        }

        public void Emit(AILEmitter Context)
        {
            Context.Generator.EmitWriteLine(Text);
        }
    }
}