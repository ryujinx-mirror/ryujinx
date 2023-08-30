namespace Ryujinx.Graphics.Shader.Translation
{
    readonly struct IoUsage
    {
        private readonly FeatureFlags _usedFeatures;

        public readonly bool UsesRtLayer => _usedFeatures.HasFlag(FeatureFlags.RtLayer);
        public readonly bool UsesViewportIndex => _usedFeatures.HasFlag(FeatureFlags.ViewportIndex);
        public readonly bool UsesViewportMask => _usedFeatures.HasFlag(FeatureFlags.ViewportMask);
        public readonly byte ClipDistancesWritten { get; }
        public readonly int UserDefinedMap { get; }

        public IoUsage(FeatureFlags usedFeatures, byte clipDistancesWritten, int userDefinedMap)
        {
            _usedFeatures = usedFeatures;
            ClipDistancesWritten = clipDistancesWritten;
            UserDefinedMap = userDefinedMap;
        }

        public readonly IoUsage Combine(IoUsage other)
        {
            return new IoUsage(
                _usedFeatures | other._usedFeatures,
                (byte)(ClipDistancesWritten | other.ClipDistancesWritten),
                UserDefinedMap | other.UserDefinedMap);
        }
    }
}
