using Ryujinx.Graphics.Gpu.State;
using System;

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

        private readonly IMacroEE _executionEngine;

        /// <summary>
        /// Creates a new instance of the GPU cached macro program.
        /// </summary>
        /// <param name="position">Macro code start position</param>
        public Macro(int position)
        {
            Position = position;

            _executionPending = false;
            _argument = 0;

            if (GraphicsConfig.EnableMacroJit)
            {
                _executionEngine = new MacroJit();
            }
            else
            {
                _executionEngine = new MacroInterpreter();
            }
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
        /// <param name="code">Program code</param>
        /// <param name="state">Current GPU state</param>
        public void Execute(ReadOnlySpan<int> code, GpuState state)
        {
            if (_executionPending)
            {
                _executionPending = false;

                _executionEngine?.Execute(code.Slice(Position), state, _argument);
            }
        }

        /// <summary>
        /// Pushes an argument to the macro call argument FIFO.
        /// </summary>
        /// <param name="argument">Argument to be pushed</param>
        public void PushArgument(int argument)
        {
            _executionEngine?.Fifo.Enqueue(argument);
        }
    }
}
