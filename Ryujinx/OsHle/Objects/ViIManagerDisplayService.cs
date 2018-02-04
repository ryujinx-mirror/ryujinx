namespace Ryujinx.OsHle.Objects
{
    class ViIManagerDisplayService
    {
        public static long CreateManagedLayer(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L); //LayerId

            return 0;
        }

        public static long AddToLayerStack(ServiceCtx Context)
        {
            return 0;
        }
    }
}