using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services.Mii.Types;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.StaticService
{
    abstract class IDatabaseService : IpcService
    {
        [Command(0)]
        // IsUpdated(SourceFlag flag) -> bool
        public ResultCode IsUpdated(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            context.ResponseData.Write(IsUpdated(flag));

            return ResultCode.Success;
        }

        [Command(1)]
        // IsFullDatabase() -> bool
        public ResultCode IsFullDatabase(ServiceCtx context)
        {
            context.ResponseData.Write(IsFullDatabase());

            return ResultCode.Success;
        }

        [Command(2)]
        // GetCount(SourceFlag flag) -> u32
        public ResultCode GetCount(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            context.ResponseData.Write(GetCount(flag));

            return ResultCode.Success;
        }

        [Command(3)]
        // Get(SourceFlag flag) -> (s32 count, buffer<nn::mii::CharInfoRawElement, 6>)
        public ResultCode Get(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            IpcBuffDesc outputBuffer = context.Request.ReceiveBuff[0];

            Span<CharInfoElement> elementsSpan = CreateSpanFromBuffer<CharInfoElement>(context, outputBuffer, true);

            ResultCode result = Get(flag, out int count, elementsSpan);

            elementsSpan = elementsSpan.Slice(0, count);

            context.ResponseData.Write(count);

            WriteSpanToBuffer(context, outputBuffer, elementsSpan);

            return result;
        }

        [Command(4)]
        // Get1(SourceFlag flag) -> (s32 count, buffer<nn::mii::CharInfo, 6>)
        public ResultCode Get1(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            IpcBuffDesc outputBuffer = context.Request.ReceiveBuff[0];

            Span<CharInfo> elementsSpan = CreateSpanFromBuffer<CharInfo>(context, outputBuffer, true);

            ResultCode result = Get1(flag, out int count, elementsSpan);

            elementsSpan = elementsSpan.Slice(0, count);

            context.ResponseData.Write(count);

            WriteSpanToBuffer(context, outputBuffer, elementsSpan);

            return result;
        }

        [Command(5)]
        // UpdateLatest(nn::mii::CharInfo old_char_info, SourceFlag flag) -> nn::mii::CharInfo
        public ResultCode UpdateLatest(ServiceCtx context)
        {
            CharInfo   oldCharInfo = context.RequestData.ReadStruct<CharInfo>();
            SourceFlag flag        = (SourceFlag)context.RequestData.ReadInt32();

            ResultCode result = UpdateLatest(oldCharInfo, flag, out CharInfo newCharInfo);

            context.ResponseData.WriteStruct(newCharInfo);

            return result;
        }

        [Command(6)]
        // BuildRandom(Age age, Gender gender, Race race) -> nn::mii::CharInfo
        public ResultCode BuildRandom(ServiceCtx context)
        {
            Age    age    = (Age)context.RequestData.ReadInt32();
            Gender gender = (Gender)context.RequestData.ReadInt32();
            Race   race   = (Race)context.RequestData.ReadInt32();

            ResultCode result = BuildRandom(age, gender, race, out CharInfo charInfo);

            context.ResponseData.WriteStruct(charInfo);

            return result;
        }

        [Command(7)]
        // BuildDefault(u32 index) -> nn::mii::CharInfoRaw
        public ResultCode BuildDefault(ServiceCtx context)
        {
            uint index = context.RequestData.ReadUInt32();

            ResultCode result = BuildDefault(index, out CharInfo charInfo);

            context.ResponseData.WriteStruct(charInfo);

            return result;
        }

        [Command(8)]
        // Get2(SourceFlag flag) -> (u32 count, buffer<nn::mii::StoreDataElement, 6>)
        public ResultCode Get2(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            IpcBuffDesc outputBuffer = context.Request.ReceiveBuff[0];

            Span<StoreDataElement> elementsSpan = CreateSpanFromBuffer<StoreDataElement>(context, outputBuffer, true);

            ResultCode result = Get2(flag, out int count, elementsSpan);

            elementsSpan = elementsSpan.Slice(0, count);

            context.ResponseData.Write(count);

            WriteSpanToBuffer(context, outputBuffer, elementsSpan);

            return result;
        }

        [Command(9)]
        // Get3(SourceFlag flag) -> (u32 count, buffer<nn::mii::StoreData, 6>)
        public ResultCode Get3(ServiceCtx context)
        {
            SourceFlag flag = (SourceFlag)context.RequestData.ReadInt32();

            IpcBuffDesc outputBuffer = context.Request.ReceiveBuff[0];

            Span<StoreData> elementsSpan = CreateSpanFromBuffer<StoreData>(context, outputBuffer, true);

            ResultCode result = Get3(flag, out int count, elementsSpan);

            elementsSpan = elementsSpan.Slice(0, count);

            context.ResponseData.Write(count);

            WriteSpanToBuffer(context, outputBuffer, elementsSpan);

            return result;
        }

        [Command(10)]
        // UpdateLatest1(nn::mii::StoreData old_store_data, SourceFlag flag) -> nn::mii::StoreData
        public ResultCode UpdateLatest1(ServiceCtx context)
        {
            StoreData  oldStoreData = context.RequestData.ReadStruct<StoreData>();
            SourceFlag flag         = (SourceFlag)context.RequestData.ReadInt32();

            ResultCode result = UpdateLatest1(oldStoreData, flag, out StoreData newStoreData);

            context.ResponseData.WriteStruct(newStoreData);

            return result;
        }

        [Command(11)]
        // FindIndex(nn::mii::CreateId create_id, bool is_special) -> s32
        public ResultCode FindIndex(ServiceCtx context)
        {
            CreateId createId  = context.RequestData.ReadStruct<CreateId>();
            bool     isSpecial = context.RequestData.ReadBoolean();

            ResultCode result = FindIndex(createId, isSpecial, out int index);

            context.ResponseData.Write(index);

            return result;
        }

        [Command(12)]
        // Move(nn::mii::CreateId create_id, s32 new_index)
        public ResultCode Move(ServiceCtx context)
        {
            CreateId createId = context.RequestData.ReadStruct<CreateId>();
            int      newIndex = context.RequestData.ReadInt32();

            return Move(createId, newIndex);
        }

        [Command(13)]
        // AddOrReplace(nn::mii::StoreData store_data)
        public ResultCode AddOrReplace(ServiceCtx context)
        {
            StoreData storeData = context.RequestData.ReadStruct<StoreData>();

            return AddOrReplace(storeData);
        }

        [Command(14)]
        // Delete(nn::mii::CreateId create_id)
        public ResultCode Delete(ServiceCtx context)
        {
            CreateId createId = context.RequestData.ReadStruct<CreateId>();

            return Delete(createId);
        }

        [Command(15)]
        // DestroyFile()
        public ResultCode DestroyFile(ServiceCtx context)
        {
            return DestroyFile();
        }

        [Command(16)]
        // DeleteFile()
        public ResultCode DeleteFile(ServiceCtx context)
        {
            return DeleteFile();
        }

        [Command(17)]
        // Format()
        public ResultCode Format(ServiceCtx context)
        {
            return Format();
        }

        [Command(18)]
        // Import(buffer<bytes, 5>)
        public ResultCode Import(ServiceCtx context)
        {
            ReadOnlySpan<byte> data = CreateByteSpanFromBuffer(context, context.Request.SendBuff[0], false);

            return Import(data);
        }

        [Command(19)]
        // Export() -> buffer<bytes, 6>
        public ResultCode Export(ServiceCtx context)
        {
            IpcBuffDesc outputBuffer = context.Request.ReceiveBuff[0];

            Span<byte> data = CreateByteSpanFromBuffer(context, outputBuffer, true);

            ResultCode result = Export(data);

            context.Memory.Write((ulong)outputBuffer.Position, data.ToArray());

            return result;
        }

        [Command(20)]
        // IsBrokenDatabaseWithClearFlag() -> bool
        public ResultCode IsBrokenDatabaseWithClearFlag(ServiceCtx context)
        {
            ResultCode result = IsBrokenDatabaseWithClearFlag(out bool isBrokenDatabase);

            context.ResponseData.Write(isBrokenDatabase);

            return result;
        }

        [Command(21)]
        // GetIndex(nn::mii::CharInfo char_info) -> s32
        public ResultCode GetIndex(ServiceCtx context)
        {
            CharInfo charInfo = context.RequestData.ReadStruct<CharInfo>();

            ResultCode result = GetIndex(charInfo, out int index);

            context.ResponseData.Write(index);

            return result;
        }

        [Command(22)] // 5.0.0+
        // SetInterfaceVersion(u32 version)
        public ResultCode SetInterfaceVersion(ServiceCtx context)
        {
            uint interfaceVersion = context.RequestData.ReadUInt32();

            SetInterfaceVersion(interfaceVersion);

            return ResultCode.Success;
        }

        [Command(23)] // 5.0.0+
        // Convert(nn::mii::Ver3StoreData ver3_store_data) -> nn::mii::CharInfo
        public ResultCode Convert(ServiceCtx context)
        {
            Ver3StoreData ver3StoreData = context.RequestData.ReadStruct<Ver3StoreData>();

            ResultCode result = Convert(ver3StoreData, out CharInfo charInfo);

            context.ResponseData.WriteStruct(charInfo);

            return result;
        }

        [Command(24)] // 7.0.0+
        // ConvertCoreDataToCharInfo(nn::mii::CoreData core_data) -> nn::mii::CharInfo
        public ResultCode ConvertCoreDataToCharInfo(ServiceCtx context)
        {
            CoreData coreData = context.RequestData.ReadStruct<CoreData>();

            ResultCode result = ConvertCoreDataToCharInfo(coreData, out CharInfo charInfo);

            context.ResponseData.WriteStruct(charInfo);

            return result;
        }

        [Command(25)] // 7.0.0+
        // ConvertCharInfoToCoreData(nn::mii::CharInfo char_info) -> nn::mii::CoreData
        public ResultCode ConvertCharInfoToCoreData(ServiceCtx context)
        {
            CharInfo charInfo = context.RequestData.ReadStruct<CharInfo>();

            ResultCode result = ConvertCharInfoToCoreData(charInfo, out CoreData coreData);

            context.ResponseData.WriteStruct(coreData);

            return result;
        }

        private Span<byte> CreateByteSpanFromBuffer(ServiceCtx context, IpcBuffDesc ipcBuff, bool isOutput)
        {
            byte[] rawData;

            if (isOutput)
            {
                rawData = new byte[ipcBuff.Size];
            }
            else
            {
                rawData = new byte[ipcBuff.Size];

                context.Memory.Read((ulong)ipcBuff.Position, rawData);
            }

            return new Span<byte>(rawData);
        }

        private Span<T> CreateSpanFromBuffer<T>(ServiceCtx context, IpcBuffDesc ipcBuff, bool isOutput) where T: unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(CreateByteSpanFromBuffer(context, ipcBuff, isOutput));
        }

        private void WriteSpanToBuffer<T>(ServiceCtx context, IpcBuffDesc ipcBuff, Span<T> span) where T: unmanaged
        {
            Span<byte> rawData = MemoryMarshal.Cast<T, byte>(span);

            context.Memory.Write((ulong)ipcBuff.Position, rawData);
        }

        protected abstract bool IsUpdated(SourceFlag flag);

        protected abstract bool IsFullDatabase();

        protected abstract uint GetCount(SourceFlag flag);

        protected abstract ResultCode Get(SourceFlag flag, out int count, Span<CharInfoElement> elements);

        protected abstract ResultCode Get1(SourceFlag flag, out int count, Span<CharInfo> elements);

        protected abstract ResultCode UpdateLatest(CharInfo oldCharInfo, SourceFlag flag, out CharInfo newCharInfo);

        protected abstract ResultCode BuildRandom(Age age, Gender gender, Race race, out CharInfo charInfo);

        protected abstract ResultCode BuildDefault(uint index, out CharInfo charInfo);

        protected abstract ResultCode Get2(SourceFlag flag, out int count, Span<StoreDataElement> elements);

        protected abstract ResultCode Get3(SourceFlag flag, out int count, Span<StoreData> elements);

        protected abstract ResultCode UpdateLatest1(StoreData oldStoreData, SourceFlag flag, out StoreData newStoreData);

        protected abstract ResultCode FindIndex(CreateId createId, bool isSpecial, out int index);

        protected abstract ResultCode Move(CreateId createId, int newIndex);

        protected abstract ResultCode AddOrReplace(StoreData storeData);

        protected abstract ResultCode Delete(CreateId createId);

        protected abstract ResultCode DestroyFile();

        protected abstract ResultCode DeleteFile();

        protected abstract ResultCode Format();

        protected abstract ResultCode Import(ReadOnlySpan<byte> data);

        protected abstract ResultCode Export(Span<byte> data);

        protected abstract ResultCode IsBrokenDatabaseWithClearFlag(out bool isBrokenDatabase);

        protected abstract ResultCode GetIndex(CharInfo charInfo, out int index);

        protected abstract void SetInterfaceVersion(uint interfaceVersion);

        protected abstract ResultCode Convert(Ver3StoreData ver3StoreData, out CharInfo charInfo);

        protected abstract ResultCode ConvertCoreDataToCharInfo(CoreData coreData, out CharInfo charInfo);

        protected abstract ResultCode ConvertCharInfoToCoreData(CharInfo charInfo, out CoreData coreData);
    }
}
