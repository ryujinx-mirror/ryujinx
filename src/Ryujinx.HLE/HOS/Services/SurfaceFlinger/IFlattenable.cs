namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    interface IFlattenable
    {
        uint GetFlattenedSize();

        uint GetFdCount();

        void Flatten(Parcel parcel);

        void Unflatten(Parcel parcel);
    }
}
