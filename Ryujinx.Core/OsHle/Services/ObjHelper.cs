using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;

namespace Ryujinx.Core.OsHle.IpcServices
{
    static class ObjHelper
    {
        public static void MakeObject(ServiceCtx Context, object Obj)
        {
            if (Context.Session is HDomain Dom)
            {
                Context.Response.ResponseObjIds.Add(Dom.GenerateObjectId(Obj));
            }
            else
            {
                HSessionObj HndData = new HSessionObj(Context.Session, Obj);

                int VHandle = Context.Ns.Os.Handles.GenerateId(HndData);

                Context.Response.HandleDesc = IpcHandleDesc.MakeMove(VHandle);
            }
        }
    }
}