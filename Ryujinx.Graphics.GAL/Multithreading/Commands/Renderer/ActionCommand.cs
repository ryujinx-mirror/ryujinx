using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer
{
    struct ActionCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.Action;
        private TableRef<Action> _action;

        public void Set(TableRef<Action> action)
        {
            _action = action;
        }

        public static void Run(ref ActionCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            command._action.Get(threaded)();
        }
    }
}
