using LibHac.Common;
using Ryujinx.HLE.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = Size)]
    struct StoreData : IStoredData<StoreData>
    {
        public const int Size = 0x44;

        public  CoreData CoreData;
        private CreateId _createId;
        public  ushort   DataCrc;
        public  ushort   DeviceCrc;

        public byte Type => CoreData.Type;

        public CreateId CreateId => _createId;

        public ResultCode InvalidData => ResultCode.InvalidStoreData;

        private void UpdateDataCrc()
        {
            DataCrc = CalculateDataCrc();
        }

        private void UpdateDeviceCrc()
        {
            DeviceCrc = CalculateDeviceCrc();
        }

        public void UpdateCrc()
        {
            UpdateDataCrc();
            UpdateDeviceCrc();
        }

        public bool IsSpecial()
        {
            return CoreData.Type == 1;
        }

        public bool IsValid()
        {
            return CoreData.IsValid() && IsValidDataCrc() && IsValidDeviceCrc();
        }

        public bool IsValidDataCrc()
        {
            return DataCrc == CalculateDataCrc();
        }

        public bool IsValidDeviceCrc()
        {
            return DeviceCrc == CalculateDeviceCrc();
        }

        private ushort CalculateDataCrc()
        {
            return Helper.CalculateCrc16BE(AsSpanWithoutCrc());
        }

        private ushort CalculateDeviceCrc()
        {
            UInt128 deviceId = Helper.GetDeviceId();

            ushort deviceIdCrc16 = Helper.CalculateCrc16BE(SpanHelpers.AsByteSpan(ref deviceId));

            return Helper.CalculateCrc16BE(AsSpanWithoutDeviceCrc(), deviceIdCrc16);
        }

        private ReadOnlySpan<byte> AsSpan()
        {
            return MemoryMarshal.AsBytes(SpanHelpers.CreateReadOnlySpan(in this, 1));
        }

        private ReadOnlySpan<byte> AsSpanWithoutCrc()
        {
            return AsSpan().Slice(0, Size - 4);
        }

        private ReadOnlySpan<byte> AsSpanWithoutDeviceCrc()
        {
            return AsSpan().Slice(0, Size - 2);
        }

        public static StoreData BuildDefault(UtilityImpl utilImpl, uint index)
        {
            StoreData result = new StoreData
            {
                _createId = utilImpl.MakeCreateId()
            };

            CoreData coreData = new CoreData();

            DefaultMii template = DefaultMii.GetDefaultMii(index);

            coreData.SetDefault();

            coreData.Nickname        = template.Nickname;
            coreData.FontRegion      = (FontRegion)template.FontRegion;
            coreData.FavoriteColor   = (byte)template.FavoriteColor;
            coreData.Gender          = (Gender)template.Gender;
            coreData.Height          = (byte)template.Height;
            coreData.Build           = (byte)template.Build;
            coreData.Type            = (byte)template.Type;
            coreData.RegionMove      = (byte)template.RegionMove;
            coreData.FacelineType    = (FacelineType)template.FacelineType;
            coreData.FacelineColor   = (FacelineColor)Helper.Ver3FacelineColorTable[template.FacelineColorVer3];
            coreData.FacelineWrinkle = (FacelineWrinkle)template.FacelineWrinkle;
            coreData.FacelineMake    = (FacelineMake)template.FacelineMake;
            coreData.HairType        = (HairType)template.HairType;
            coreData.HairColor       = (CommonColor)Helper.Ver3HairColorTable[template.HairColorVer3];
            coreData.HairFlip        = (HairFlip)template.HairFlip;
            coreData.EyeType         = (EyeType)template.EyeType;
            coreData.EyeColor        = (CommonColor)Helper.Ver3EyeColorTable[template.EyeColorVer3];
            coreData.EyeScale        = (byte)template.EyeScale;
            coreData.EyeAspect       = (byte)template.EyeAspect;
            coreData.EyeRotate       = (byte)template.EyeRotate;
            coreData.EyeX            = (byte)template.EyeX;
            coreData.EyeY            = (byte)template.EyeY;
            coreData.EyebrowType     = (EyebrowType)template.EyebrowType;
            coreData.EyebrowColor    = (CommonColor)Helper.Ver3HairColorTable[template.EyebrowColorVer3];
            coreData.EyebrowScale    = (byte)template.EyebrowScale;
            coreData.EyebrowAspect   = (byte)template.EyebrowAspect;
            coreData.EyebrowRotate   = (byte)template.EyebrowRotate;
            coreData.EyebrowX        = (byte)template.EyebrowX;
            coreData.EyebrowY        = (byte)template.EyebrowY;
            coreData.NoseType        = (NoseType)template.NoseType;
            coreData.NoseScale       = (byte)template.NoseScale;
            coreData.NoseY           = (byte)template.NoseY;
            coreData.MouthType       = (MouthType)template.MouthType;
            coreData.MouthColor      = (CommonColor)Helper.Ver3MouthColorTable[template.MouthColorVer3];
            coreData.MouthScale      = (byte)template.MouthScale;
            coreData.MouthAspect     = (byte)template.MouthAspect;
            coreData.MouthY          = (byte)template.MouthY;
            coreData.BeardColor      = (CommonColor)Helper.Ver3HairColorTable[template.BeardColorVer3];
            coreData.BeardType       = (BeardType)template.BeardType;
            coreData.MustacheType    = (MustacheType)template.MustacheType;
            coreData.MustacheScale   = (byte)template.MustacheScale;
            coreData.MustacheY       = (byte)template.MustacheY;
            coreData.GlassType       = (GlassType)template.GlassType;
            coreData.GlassColor      = (CommonColor)Helper.Ver3GlassColorTable[template.GlassColorVer3];
            coreData.GlassScale      = (byte)template.GlassScale;
            coreData.GlassY          = (byte)template.GlassY;
            coreData.MoleType        = (MoleType)template.MoleType;
            coreData.MoleScale       = (byte)template.MoleScale;
            coreData.MoleX           = (byte)template.MoleX;
            coreData.MoleY           = (byte)template.MoleY;

            result.CoreData = coreData;

            result.UpdateCrc();

            return result;
        }

        public static StoreData BuildRandom(UtilityImpl utilImpl, Age age, Gender gender, Race race)
        {
            return BuildFromCoreData(utilImpl, CoreData.BuildRandom(utilImpl, age, gender, race));
        }

        public static StoreData BuildFromCoreData(UtilityImpl utilImpl, CoreData coreData)
        {
            StoreData result = new StoreData
            {
                CoreData  = coreData,
                _createId = utilImpl.MakeCreateId()
            };

            result.UpdateCrc();

            return result;
        }

        public void SetFromStoreData(StoreData storeData)
        {
            this = storeData;
        }

        public void SetSource(Source source)
        {
            // Only implemented for Element variants.
        }

        public static bool operator ==(StoreData x, StoreData y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(StoreData x, StoreData y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is StoreData storeData && Equals(storeData);
        }

        public bool Equals(StoreData cmpObj)
        {
            if (!cmpObj.IsValid())
            {
                return false;
            }

            bool result = true;

            result &= CreateId == cmpObj.CreateId;
            result &= CoreData == cmpObj.CoreData;
            result &= DataCrc == cmpObj.DataCrc;
            result &= DeviceCrc == cmpObj.DeviceCrc;

            return result;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(CreateId);
            hashCode.Add(CoreData);
            hashCode.Add(DataCrc);
            hashCode.Add(DeviceCrc);

            return hashCode.ToHashCode();
        }
    }
}
