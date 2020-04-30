using Ryujinx.Common.Utilities;
using System.IO;

namespace Ryujinx.Debugger.Profiler
{
    public class ProfilerConfiguration
    {
        public bool   Enabled    { get; private set; }
        public string DumpPath   { get; private set; }
        public float  UpdateRate { get; private set; }
        public int    MaxLevel   { get; private set; }
        public int    MaxFlags   { get; private set; }
        public float  History    { get; private set; }

        /// <summary>
        /// Loads a configuration file from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static ProfilerConfiguration Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Profiler configuration file {path} not found");
            }

            return JsonHelper.DeserializeFromFile<ProfilerConfiguration>(path);
        }
    }
}
