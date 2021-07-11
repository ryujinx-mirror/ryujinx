using Ryujinx.Graphics.Device;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Macro Execution Engine interface.
    /// </summary>
    interface IMacroEE
    {
        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        Queue<int> Fifo { get; }

        /// <summary>
        /// Should execute the GPU Macro code being passed.
        /// </summary>
        /// <param name="code">Code to be executed</param>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument to be passed to the GPU Macro</param>
        void Execute(ReadOnlySpan<int> code, IDeviceState state, int arg0);
    }
}
