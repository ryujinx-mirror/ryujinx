using LibHac.Fs;
using LibHac.Fs.NcaUtils;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.HLE.HOS.Services.Time.TimeZone.TimeZoneRule;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    class TimeZoneContentManager
    {
        private const long TimeZoneBinaryTitleId = 0x010000000000080E;

        private Switch   _device;
        private string[] _locationNameCache;

        public TimeZoneManager Manager { get; private set; }

        public TimeZoneContentManager()
        {
            Manager = new TimeZoneManager();
        }

        internal void Initialize(TimeManager timeManager, Switch device)
        {
            _device = device;

            InitializeLocationNameCache();

            SteadyClockTimePoint timeZoneUpdatedTimePoint = timeManager.StandardSteadyClock.GetCurrentTimePoint(null);

            ResultCode result = GetTimeZoneBinary("UTC", out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

            if (result == ResultCode.Success)
            {
                // TODO: Read TimeZoneVersion from sysarchive.
                timeManager.SetupTimeZoneManager("UTC", timeZoneUpdatedTimePoint, (uint)_locationNameCache.Length, new UInt128(), timeZoneBinaryStream);

                ncaFile.Dispose();
            }
            else
            {
                // In the case the user don't have the timezone system archive, we just mark the manager as initialized.
                Manager.MarkInitialized();
            }
        }

        private void InitializeLocationNameCache()
        {
            if (HasTimeZoneBinaryTitle())
            {
                using (IStorage ncaFileStream = new LocalStorage(_device.FileSystem.SwitchPathToSystemPath(GetTimeZoneBinaryTitleContentPath()), FileAccess.Read, FileMode.Open))
                {
                    Nca         nca              = new Nca(_device.System.KeySet, ncaFileStream);
                    IFileSystem romfs            = nca.OpenFileSystem(NcaSectionType.Data, _device.System.FsIntegrityCheckLevel);
                    Stream      binaryListStream = romfs.OpenFile("binaryList.txt", OpenMode.Read).AsStream();

                    StreamReader reader = new StreamReader(binaryListStream);

                    List<string> locationNameList = new List<string>();

                    string locationName;
                    while ((locationName = reader.ReadLine()) != null)
                    {
                        locationNameList.Add(locationName);
                    }

                    _locationNameCache = locationNameList.ToArray();
                }
            }
            else
            {
                _locationNameCache = new string[0];

                Logger.PrintWarning(LogClass.ServiceTime, "TimeZoneBinary system title not found! TimeZone conversions will not work, provide the system archive to fix this warning. (See https://github.com/Ryujinx/Ryujinx#requirements for more informations)");
            }
        }

        private bool IsLocationNameValid(string locationName)
        {
            foreach (string cachedLocationName in _locationNameCache)
            {
                if (cachedLocationName.Equals(locationName))
                {
                    return true;
                }
            }

            return false;
        }

        public ResultCode SetDeviceLocationName(string locationName)
        {
            ResultCode result = GetTimeZoneBinary(locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

            if (result == ResultCode.Success)
            {
                result = Manager.SetDeviceLocationNameWithTimeZoneRule(locationName, timeZoneBinaryStream);

                ncaFile.Dispose();
            }

            return result;
        }

        public ResultCode LoadLocationNameList(uint index, out string[] outLocationNameArray, uint maxLength)
        {
            List<string> locationNameList = new List<string>();

            for (int i = 0; i < _locationNameCache.Length && i < maxLength; i++)
            {
                if (i < index)
                {
                    continue;
                }

                string locationName = _locationNameCache[i];

                // If the location name is too long, error out.
                if (locationName.Length > 0x24)
                {
                    outLocationNameArray = new string[0];

                    return ResultCode.LocationNameTooLong;
                }

                locationNameList.Add(locationName);
            }

            outLocationNameArray = locationNameList.ToArray();

            return ResultCode.Success;
        }

        public string GetTimeZoneBinaryTitleContentPath()
        {
            return _device.System.ContentManager.GetInstalledContentPath(TimeZoneBinaryTitleId, StorageId.NandSystem, ContentType.Data);
        }

        public bool HasTimeZoneBinaryTitle()
        {
            return !string.IsNullOrEmpty(GetTimeZoneBinaryTitleContentPath());
        }

        internal ResultCode GetTimeZoneBinary(string locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile)
        {
            timeZoneBinaryStream = null;
            ncaFile              = null;

            if (!IsLocationNameValid(locationName))
            {
                return ResultCode.TimeZoneNotFound;
            }

            ncaFile = new LocalStorage(_device.FileSystem.SwitchPathToSystemPath(GetTimeZoneBinaryTitleContentPath()), FileAccess.Read, FileMode.Open);

            Nca         nca   = new Nca(_device.System.KeySet, ncaFile);
            IFileSystem romfs = nca.OpenFileSystem(NcaSectionType.Data, _device.System.FsIntegrityCheckLevel);

            timeZoneBinaryStream = romfs.OpenFile($"/zoneinfo/{locationName}", OpenMode.Read).AsStream();

            return ResultCode.Success;
        }

        internal ResultCode LoadTimeZoneRule(out TimeZoneRule outRules, string locationName)
        {
            outRules = new TimeZoneRule
            {
                Ats   = new long[TzMaxTimes],
                Types = new byte[TzMaxTimes],
                Ttis  = new TimeTypeInfo[TzMaxTypes],
                Chars = new char[TzCharsArraySize]
            };

            if (!HasTimeZoneBinaryTitle())
            {
                throw new InvalidSystemResourceException($"TimeZoneBinary system title not found! Please provide it. (See https://github.com/Ryujinx/Ryujinx#requirements for more informations)");
            }

            ResultCode result = GetTimeZoneBinary(locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

            if (result == ResultCode.Success)
            {
                result = Manager.ParseTimeZoneRuleBinary(out outRules, timeZoneBinaryStream);

                ncaFile.Dispose();
            }

            return result;
        }
    }
}