using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// GPU macro program.
    /// </summary>
    struct Macro
    {
        /// <summary>
        /// Word offset of the code on the code memory.
        /// </summary>
        public int Position { get; }

        private bool _executionPending;
        private int _argument;

        private readonly MacroInterpreter _interpreter;

        /// <summary>
        /// Creates a new instance of the GPU cached macro program.
        /// </summary>
        /// <param name="position">Macro code start position</param>
        public Macro(int position)
        {
            Position = position;

            _executionPending = false;
            _argument = 0;

            _interpreter = new MacroInterpreter();
        }

        /// <summary>
        /// Sets the first argument for the macro call.
        /// </summary>
        /// <param name="argument">First argument</param>
        public void StartExecution(int argument)
        {
            _argument = argument;

            _executionPending = true;
        }

        /// <summary>
        /// Starts executing the macro program code.
        /// </summary>
        /// <param name="mme">Program code</param>
        /// <param name="state">Current GPU state</param>
        public void Execute(int[] mme, ShadowRamControl shadowCtrl, GpuState state)
        {
            if (_executionPending)
            {
                _executionPending = false;

                _interpreter?.Execute(mme, Position, _argument, shadowCtrl, state);
            }
        }

        /// <summary>
        /// Pushes an argument to the macro call argument FIFO.
        /// </summary>
        /// <param name="argument">Argument to be pushed</param>
        public void PushArgument(int argument)
        {
            _interpreter?.Fifo.Enqueue(argument);
        }
    }
}
