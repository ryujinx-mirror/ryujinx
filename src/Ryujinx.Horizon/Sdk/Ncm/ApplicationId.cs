namespace Ryujinx.Horizon.Sdk.Ncm
{
    public readonly struct ApplicationId
    {
        public readonly ulong Id;

        public static int Length => sizeof(ulong);

        public static ApplicationId First => new(0x0100000000010000);

        public static ApplicationId Last => new(0x01FFFFFFFFFFFFFF);

        public static ApplicationId Invalid => new(0);

        public bool IsValid => Id >= First.Id && Id <= Last.Id;

        public ApplicationId(ulong id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            return obj is ApplicationId applicationId && applicationId.Equals(this);
        }

        public bool Equals(ApplicationId other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ApplicationId lhs, ApplicationId rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ApplicationId lhs, ApplicationId rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override string ToString()
        {
            return $"0x{Id:x}";
        }
    }
}
