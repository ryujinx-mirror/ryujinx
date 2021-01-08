using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Configuration;
using System;
using System.Numerics;

namespace Ryujinx.Modules.Motion
{
    public class MotionDevice
    {
        public Vector3 Gyroscope     { get; private set; }
        public Vector3 Accelerometer { get; private set; }
        public Vector3 Rotation      { get; private set; }
        public float[] Orientation   { get; private set; }

        private readonly Client _motionSource;

        public MotionDevice(Client motionSource)
        {
            _motionSource = motionSource;
        }

        public void RegisterController(PlayerIndex player)
        {
            InputConfig config = ConfigurationState.Instance.Hid.InputConfig.Value.Find(x => x.PlayerIndex == player);

            if (config != null && config.EnableMotion)
            {
                string host = config.DsuServerHost;
                int    port = config.DsuServerPort;

                _motionSource.RegisterClient((int)player, host, port);
                _motionSource.RequestData((int)player, config.Slot);

                if (config.ControllerType == ControllerType.JoyconPair && !config.MirrorInput)
                {
                    _motionSource.RequestData((int)player, config.AltSlot);
                }
            }
        }

        public void Poll(InputConfig config, int slot)
        {
            Orientation = new float[9];

            if (!config.EnableMotion || !_motionSource.TryGetData((int)config.PlayerIndex, slot, out MotionInput input))
            {
                Accelerometer = new Vector3();
                Gyroscope     = new Vector3();

                return;
            }

            Gyroscope     = Truncate(input.Gyroscrope * 0.0027f, 3);
            Accelerometer = Truncate(input.Accelerometer,        3);
            Rotation      = Truncate(input.Rotation * 0.0027f,   3);

            Matrix4x4 orientation = input.GetOrientation();

            Orientation[0] = Math.Clamp(orientation.M11, -1f, 1f);
            Orientation[1] = Math.Clamp(orientation.M12, -1f, 1f);
            Orientation[2] = Math.Clamp(orientation.M13, -1f, 1f);
            Orientation[3] = Math.Clamp(orientation.M21, -1f, 1f);
            Orientation[4] = Math.Clamp(orientation.M22, -1f, 1f);
            Orientation[5] = Math.Clamp(orientation.M23, -1f, 1f);
            Orientation[6] = Math.Clamp(orientation.M31, -1f, 1f);
            Orientation[7] = Math.Clamp(orientation.M32, -1f, 1f);
            Orientation[8] = Math.Clamp(orientation.M33, -1f, 1f);
        }

        private static Vector3 Truncate(Vector3 value, int decimals)
        {
            float power = MathF.Pow(10, decimals);

            value.X = float.IsNegative(value.X) ? MathF.Ceiling(value.X * power) / power : MathF.Floor(value.X * power) / power;
            value.Y = float.IsNegative(value.Y) ? MathF.Ceiling(value.Y * power) / power : MathF.Floor(value.Y * power) / power;
            value.Z = float.IsNegative(value.Z) ? MathF.Ceiling(value.Z * power) / power : MathF.Floor(value.Z * power) / power;

            return value;
        }
    }
}