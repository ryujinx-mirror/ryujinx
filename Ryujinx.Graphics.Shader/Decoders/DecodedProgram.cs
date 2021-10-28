using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    struct DecodedProgram : IEnumerable<DecodedFunction>
    {
        public DecodedFunction MainFunction { get; }
        private readonly IReadOnlyDictionary<ulong, DecodedFunction> _functions;
        private readonly List<DecodedFunction> _functionsWithId;
        public int FunctionsWithIdCount => _functionsWithId.Count;

        public DecodedProgram(DecodedFunction mainFunction, IReadOnlyDictionary<ulong, DecodedFunction> functions)
        {
            MainFunction = mainFunction;
            _functions = functions;
            _functionsWithId = new List<DecodedFunction>();
        }

        public DecodedFunction GetFunctionByAddress(ulong address)
        {
            if (_functions.TryGetValue(address, out DecodedFunction function))
            {
                return function;
            }

            return null;
        }

        public DecodedFunction GetFunctionById(int id)
        {
            if ((uint)id >= (uint)_functionsWithId.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return _functionsWithId[id];
        }

        public void AddFunctionAndSetId(DecodedFunction function)
        {
            function.Id = _functionsWithId.Count;
            _functionsWithId.Add(function);
        }

        public IEnumerator<DecodedFunction> GetEnumerator()
        {
            return _functions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}