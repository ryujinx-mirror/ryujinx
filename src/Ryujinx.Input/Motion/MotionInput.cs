using Ryujinx.Input.Motion;
using System;
using System.Numerics;

namespace Ryujinx.Input
{
    public class MotionInput
    {
        public ulong TimeStamp { get; set; }
        public Vector3 Accelerometer { get; set; }
        public Vector3 Gyroscrope { get; set; }
        public Vector3 Rotation { get; set; }

        private readonly MotionSensorFilter _filter;

        public MotionInput()
        {
            TimeStamp = 0;
            Accelerometer = new Vector3();
            Gyroscrope = new Vector3();
            Rotation = new Vector3();

            // TODO: RE the correct filter.
            _filter = new MotionSensorFilter(0f);
        }

        public void Update(Vector3 accel, Vector3 gyro, ulong timestamp, int sensitivity, float deadzone)
        {
            if (TimeStamp != 0)
            {
                Accelerometer = -accel;

                if (gyro.Length() < deadzone)
                {
                    gyro = Vector3.Zero;
                }

                gyro *= (sensitivity / 100f);

                Gyroscrope = gyro;

                float deltaTime = MathF.Abs((long)(timestamp - TimeStamp) / 1000000f);

                Vector3 deltaGyro = gyro * deltaTime;

                Rotation += deltaGyro;

                _filter.SamplePeriod = deltaTime;
                _filter.Update(accel, DegreeToRad(gyro));
            }

            TimeStamp = timestamp;
        }

        public Matrix4x4 GetOrientation()
        {
            return Matrix4x4.CreateFromQuaternion(_filter.Quaternion);
        }

        private static Vector3 DegreeToRad(Vector3 degree)
        {
            return degree * (MathF.PI / 180);
        }
    }
}
