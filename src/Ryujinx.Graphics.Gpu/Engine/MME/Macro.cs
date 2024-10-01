using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Engine.GPFifo;
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

        private IMacroEE _executionEngine;
        private bool _executionPending;
        private int _argument;
        private MacroHLEFunctionName _hleFunction;

        /// <summary>
        /// Creates a new instance of the GPU cached macro program.
        /// </summary>
        /// <param name="position">Macro code start position</param>
        public Macro(int position)
        {
            Position = position;

            _executionEngine = null;
            _executionPending = false;
            _argument = 0;
            _hleFunction = MacroHLEFunctionName.None;
        }

        /// <summary>
        /// Sets the first argument for the macro call.
        /// </summary>
        /// <param name="context">GPU context where the macro code is being executed</param>
        /// <param name="processor">GPU GP FIFO command processor</param>
        /// <param name="code">Code to be executed</param>
        /// <param name="argument">First argument</param>
        public void StartExecution(GpuContext context, GPFifoProcessor processor, ReadOnlySpan<int> code, int argument)
        {
            _argument = argument;

            _executionPending = true;

            if (_executionEngine == null)
            {
                if (GraphicsConfig.EnableMacroHLE && MacroHLETable.TryGetMacroHLEFunction(code[Position..], context.Capabilities, out _hleFunction))
                {
                    _executionEngine = new MacroHLE(processor, _hleFunction);
                }
                else if (GraphicsConfig.EnableMacroJit)
                {
                    _executionEngine = new MacroJit();
                }
                else
                {
                    _executionEngine = new MacroInterpreter();
                }
            }

            // We don't consume the parameter buffer value, so we don't need to flush it.
            // Doing so improves performance if the value was written by a GPU shader.
            if (_hleFunction == MacroHLEFunctionName.DrawElementsIndirect)
            {
                context.GPFifo.SetFlushSkips(1);
            }
            else if (_hleFunction == MacroHLEFunctionName.MultiDrawElementsIndirectCount)
            {
                context.GPFifo.SetFlushSkips(2);
            }
        }

        /// <summary>
        /// Starts executing the macro program code.
        /// </summary>
        /// <param name="code">Program code</param>
        /// <param name="state">Current GPU state</param>
        public void Execute(ReadOnlySpan<int> code, IDeviceState state)
        {
            if (_executionPending)
            {
                _executionPending = false;
                _executionEngine?.Execute(code[Position..], state, _argument);
            }
        }

        /// <summary>
        /// Pushes an argument to the macro call argument FIFO.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the command word is located</param>
        /// <param name="argument">Argument to be pushed</param>
        public readonly void PushArgument(ulong gpuVa, int argument)
        {
            _executionEngine?.Fifo.Enqueue(new FifoWord(gpuVa, argument));
        }
    }
}
