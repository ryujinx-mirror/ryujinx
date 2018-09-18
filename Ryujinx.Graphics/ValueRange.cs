namespace Ryujinx.Graphics
{
    struct ValueRange<T>
    {
        public long Start { get; private set; }
        public long End   { get; private set; }

        public T Value { get; set; }

        public ValueRange(long Start, long End, T Value = default(T))
        {
            this.Start = Start;
            this.End   = End;
            this.Value = Value;
        }
    }
}