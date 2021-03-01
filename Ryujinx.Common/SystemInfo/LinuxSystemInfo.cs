using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using Ryujinx.Common.Logging;

namespace Ryujinx.Common.SystemInfo
{
    [SupportedOSPlatform("linux")]
    class LinuxSystemInfo : SystemInfo
    {
        internal LinuxSystemInfo()
        {
            string cpuName = GetCpuidCpuName();

            if (cpuName == null)
            {
                var cpuDict = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["model name"] = null,
                    ["Processor"] = null,
                    ["Hardware"] = null
                };

                ParseKeyValues("/proc/cpuinfo", cpuDict);

                cpuName = cpuDict["model name"] ?? cpuDict["Processor"] ?? cpuDict["Hardware"] ?? "Unknown";
            }

            var memDict = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["MemTotal"] = null,
                ["MemAvailable"] = null
            };

            ParseKeyValues("/proc/meminfo", memDict);

            // Entries are in KB
            ulong.TryParse(memDict["MemTotal"]?.Split(' ')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong totalKB);
            ulong.TryParse(memDict["MemAvailable"]?.Split(' ')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong availableKB);

            CpuName = $"{cpuName} ; {LogicalCoreCount} logical";
            RamTotal = totalKB * 1024;
            RamAvailable = availableKB * 1024;
        }

        private static void ParseKeyValues(string filePath, Dictionary<string, string> itemDict)
        {
            if (!File.Exists(filePath))
            {
                Logger.Error?.Print(LogClass.Application, $"File \"{filePath}\" not found");

                return;
            }

            int count = itemDict.Count;

            using (StreamReader file = new StreamReader(filePath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] kvPair = line.Split(':', 2, StringSplitOptions.TrimEntries);

                    if (kvPair.Length < 2) continue;

                    string key = kvPair[0];

                    if (itemDict.TryGetValue(key, out string value) && value == null)
                    {
                        itemDict[key] = kvPair[1];

                        if (--count <= 0) break;
                    }
                }
            }
        }
    }
}