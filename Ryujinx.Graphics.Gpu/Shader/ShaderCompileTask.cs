using Ryujinx.Graphics.GAL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Graphics.Gpu.Shader
{
    delegate bool ShaderCompileTaskCallback(bool success, ShaderCompileTask task);

    /// <summary>
    /// A class that represents a shader compilation.
    /// </summary>
    class ShaderCompileTask
    {
        private bool _compiling;

        private Task _programsTask;
        private IProgram _program;

        private ShaderCompileTaskCallback _action;
        private AutoResetEvent _taskDoneEvent;

        /// <summary>
        /// Create a new shader compile task, with an event to signal whenever a subtask completes.
        /// </summary>
        /// <param name="taskDoneEvent">Event to signal when a subtask completes</param>
        public ShaderCompileTask(AutoResetEvent taskDoneEvent)
        {
            _taskDoneEvent = taskDoneEvent;
        }

        /// <summary>
        /// Check the completion status of the shader compile task, and run callbacks on step completion.
        /// Calling this periodically is required to progress through steps of the compilation.
        /// </summary>
        /// <returns>True if the task is complete, false if it is in progress</returns>
        public bool IsDone()
        {
            if (_compiling)
            {
                ProgramLinkStatus status = _program.CheckProgramLink(false);

                if (status != ProgramLinkStatus.Incomplete)
                {
                    return _action(status == ProgramLinkStatus.Success, this);
                }
            }
            else
            {
                // Waiting on the task.

                if (_programsTask.IsCompleted)
                {
                    return _action(true, this);
                }
            }

            return false;
        }

        /// <summary>
        /// Run a callback when the specified task has completed.
        /// </summary>
        /// <param name="task">The task object that needs to complete</param>
        /// <param name="action">The action to perform when it is complete</param>
        public void OnTask(Task task, ShaderCompileTaskCallback action)
        {
            _compiling = false;

            _programsTask = task;
            _action = action;

            task.ContinueWith(task => _taskDoneEvent.Set());
        }

        /// <summary>
        /// Run a callback when the specified program has been linked.
        /// </summary>
        /// <param name="task">The program that needs to be linked</param>
        /// <param name="action">The action to perform when linking is complete</param>
        public void OnCompiled(IProgram program, ShaderCompileTaskCallback action)
        {
            _compiling = true;

            _program = program;
            _action = action;

            if (program == null)
            {
                action(false, this);
            }
        }
    }
}
