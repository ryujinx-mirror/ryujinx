using ARMeilleure.State;

namespace ARMeilleure.Translation
{
    public interface ITranslator
    {
        void Execute(IExecutionContext context, ulong address);
    }
}