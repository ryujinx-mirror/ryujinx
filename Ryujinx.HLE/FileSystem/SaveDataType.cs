namespace Ryujinx.HLE.FileSystem
{
    enum SaveDataType : byte
    {
        SystemSaveData,
        SaveData,
        BcatDeliveryCacheStorage,
        DeviceSaveData,
        TemporaryStorage,
        CacheStorage
    }
}