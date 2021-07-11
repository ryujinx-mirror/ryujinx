namespace Ryujinx.Graphics.Gpu.Engine
{
    /// <summary>
    /// MME shadow RAM control mode.
    /// </summary>
    enum SetMmeShadowRamControlMode
    {
        MethodTrack = 0,
        MethodTrackWithFilter = 1,
        MethodPassthrough = 2,
        MethodReplay = 3,
    }
}