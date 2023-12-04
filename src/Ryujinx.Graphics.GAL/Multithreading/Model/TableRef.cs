namespace Ryujinx.Graphics.GAL.Multithreading.Model
{
    readonly struct TableRef<T>
    {
        private readonly int _index;

        public TableRef(ThreadedRenderer renderer, T reference)
        {
            _index = renderer.AddTableRef(reference);
        }

        public T Get(ThreadedRenderer renderer)
        {
            return (T)renderer.RemoveTableRef(_index);
        }

        public T2 GetAs<T2>(ThreadedRenderer renderer) where T2 : T
        {
            return (T2)renderer.RemoveTableRef(_index);
        }
    }
}
