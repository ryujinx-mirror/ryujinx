using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.State;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Represents a Macro Just-in-Time compiler execution context.
    /// </summary>
    class MacroJitContext
    {
        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        public Queue<int> Fifo { get; } = new Queue<int>();

        /// <summary>
        /// Fetches a arguments from the arguments FIFO.
        /// </summary>
        /// <returns></returns>
        public int FetchParam()
        {
            if (!Fifo.TryDequeue(out int value))
            {
                Logger.Warning?.Print(LogClass.Gpu, "Macro attempted to fetch an inexistent argument.");

                return 0;
            }

            return value;
        }

        /// <summary>
        /// Reads data from a GPU register.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="reg">Register offset to read</param>
        /// <returns>GPU register value</returns>
        public static int Read(GpuState state, int reg)
        {
            return state.Read(reg);
        }

        /// <summary>
        /// Performs a GPU method call.
        /// </summary>
        /// <param name="value">Call argument</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="methAddr">Address, in words, of the method</param>
        public static void Send(int value, GpuState state, int methAddr)
        {
            MethodParams meth = new MethodParams(methAddr, value);

            state.CallMethod(meth);
        }
    }
}
