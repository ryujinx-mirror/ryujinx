using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Tamper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS
{
    public class TamperMachine
    {
        // Atmosphere specifies a delay of 83 milliseconds between the execution of the last
        // cheat and the re-execution of the first one.
        private const int TamperMachineSleepMs = 1000 / 12;

        private Thread _tamperThread = null;
        private ConcurrentQueue<ITamperProgram> _programs = new ConcurrentQueue<ITamperProgram>();
        private long _pressedKeys = 0;

        private void Activate()
        {
            if (_tamperThread == null || !_tamperThread.IsAlive)
            {
                _tamperThread = new Thread(this.TamperRunner);
                _tamperThread.Name = "HLE.TamperMachine";
                _tamperThread.Start();
            }
        }

        internal void InstallAtmosphereCheat(IEnumerable<string> rawInstructions, ProcessTamperInfo info, ulong exeAddress)
        {
            if (!CanInstallOnPid(info.Process.Pid))
            {
                return;
            }

            ITamperedProcess tamperedProcess = new TamperedKProcess(info.Process);
            AtmosphereCompiler compiler = new AtmosphereCompiler();
            ITamperProgram program = compiler.Compile(rawInstructions, exeAddress, info.HeapAddress, tamperedProcess);

            if (program != null)
            {
                _programs.Enqueue(program);
            }

            Activate();
        }

        private bool CanInstallOnPid(long pid)
        {
            // Do not allow tampering of kernel processes.
            if (pid < KernelConstants.InitialProcessId)
            {
                Logger.Warning?.Print(LogClass.TamperMachine, $"Refusing to tamper kernel process {pid}");

                return false;
            }

            return true;
        }

        private bool IsProcessValid(ITamperedProcess process)
        {
            return process.State != ProcessState.Crashed && process.State != ProcessState.Exiting && process.State != ProcessState.Exited;
        }

        private void TamperRunner()
        {
            Logger.Info?.Print(LogClass.TamperMachine, "TamperMachine thread running");

            int sleepCounter = 0;

            while (true)
            {
                // Sleep to not consume too much CPU.
                if (sleepCounter == 0)
                {
                    sleepCounter = _programs.Count;
                    Thread.Sleep(TamperMachineSleepMs);
                }
                else
                {
                    sleepCounter--;
                }

                if (!AdvanceTamperingsQueue())
                {
                    // No more work to be done.

                    Logger.Info?.Print(LogClass.TamperMachine, "TamperMachine thread exiting");

                    return;
                }
            }
        }

        private bool AdvanceTamperingsQueue()
        {
            if (!_programs.TryDequeue(out ITamperProgram program))
            {
                // No more programs in the queue.
                return false;
            }

            // Check if the process is still suitable for running the tamper program.
            if (!IsProcessValid(program.Process))
            {
                // Exit without re-enqueuing the program because the process is no longer valid.
                return true;
            }

            // Re-enqueue the tampering program because the process is still valid.
            _programs.Enqueue(program);

            Logger.Debug?.Print(LogClass.TamperMachine, "Running tampering program");

            try
            {
                ControllerKeys pressedKeys = (ControllerKeys)Thread.VolatileRead(ref _pressedKeys);
                program.Execute(pressedKeys);
            }
            catch (CodeRegionTamperedException ex)
            {
                Logger.Debug?.Print(LogClass.TamperMachine, $"Prevented tampering program from modifing code memory");

                if (!String.IsNullOrEmpty(ex.Message))
                {
                    Logger.Debug?.Print(LogClass.TamperMachine, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug?.Print(LogClass.TamperMachine, $"The tampering program crashed, this can happen while the game is starting");

                if (!String.IsNullOrEmpty(ex.Message))
                {
                    Logger.Debug?.Print(LogClass.TamperMachine, ex.Message);
                }
            }

            return true;
        }

        public void UpdateInput(List<GamepadInput> gamepadInputs)
        {
            // Look for the input of the player one or the handheld.
            foreach (GamepadInput input in gamepadInputs)
            {
                if (input.PlayerId == PlayerIndex.Player1 || input.PlayerId == PlayerIndex.Handheld)
                {
                    Thread.VolatileWrite(ref _pressedKeys, (long)input.Buttons);

                    return;
                }
            }

            // Clear the input because player one is not conected.
            Thread.VolatileWrite(ref _pressedKeys, 0);
        }
    }
}
