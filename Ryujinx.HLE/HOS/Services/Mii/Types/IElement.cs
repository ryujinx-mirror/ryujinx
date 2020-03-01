namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    interface IElement
    {
        void SetFromStoreData(StoreData storeData);

        void SetSource(Source source);
    }
}
