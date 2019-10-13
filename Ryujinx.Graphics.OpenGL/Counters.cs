using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Counters
    {
        private int[] _queryObjects;

        private ulong[] _accumulatedCounters;

        public Counters()
        {
            int count = Enum.GetNames(typeof(CounterType)).Length;

            _queryObjects = new int[count];

            _accumulatedCounters = new ulong[count];
        }

        public void Initialize()
        {
            for (int index = 0; index < _queryObjects.Length; index++)
            {
                int handle = GL.GenQuery();

                _queryObjects[index] = handle;

                CounterType type = (CounterType)index;

                GL.BeginQuery(GetTarget(type), handle);
            }
        }

        public ulong GetCounter(CounterType type)
        {
            UpdateAccumulatedCounter(type);

            return _accumulatedCounters[(int)type];
        }

        public void ResetCounter(CounterType type)
        {
            UpdateAccumulatedCounter(type);

            _accumulatedCounters[(int)type] = 0;
        }

        private void UpdateAccumulatedCounter(CounterType type)
        {
            int handle = _queryObjects[(int)type];

            QueryTarget target = GetTarget(type);

            GL.EndQuery(target);

            GL.GetQueryObject(handle, GetQueryObjectParam.QueryResult, out long result);

            _accumulatedCounters[(int)type] += (ulong)result;

            GL.BeginQuery(target, handle);
        }

        private static QueryTarget GetTarget(CounterType type)
        {
            switch (type)
            {
                case CounterType.SamplesPassed:                      return QueryTarget.SamplesPassed;
                case CounterType.PrimitivesGenerated:                return QueryTarget.PrimitivesGenerated;
                case CounterType.TransformFeedbackPrimitivesWritten: return QueryTarget.TransformFeedbackPrimitivesWritten;
            }

            return QueryTarget.SamplesPassed;
        }
    }
}
