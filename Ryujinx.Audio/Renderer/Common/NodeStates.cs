//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Ryujinx.Audio.Renderer.Utils;
using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Common
{
    public class NodeStates
    {
        private class Stack
        {
            private Memory<int> _storage;
            private int _index;

            private int _nodeCount;

            public void Reset(Memory<int> storage, int nodeCount)
            {
                Debug.Assert(storage.Length * sizeof(int) >= CalcBufferSize(nodeCount));

                _storage = storage;
                _index = 0;
                _nodeCount = nodeCount;
            }

            public int GetCurrentCount()
            {
                return _index;
            }

            public void Push(int data)
            {
                Debug.Assert(_index + 1 <= _nodeCount);

                _storage.Span[_index++] = data;
            }

            public int Pop()
            {
                Debug.Assert(_index > 0);

                return _storage.Span[--_index];
            }

            public int Top()
            {
                return _storage.Span[_index - 1];
            }

            public static int CalcBufferSize(int nodeCount)
            {
                return nodeCount * sizeof(int);
            }
        }

        private int _nodeCount;
        private EdgeMatrix _discovered;
        private EdgeMatrix _finished;
        private Memory<int> _resultArray;
        private Stack _stack;
        private int _tsortResultIndex;

        private enum NodeState : byte
        {
            Unknown,
            Discovered,
            Finished
        }

        public NodeStates()
        {
            _stack = new Stack();
            _discovered = new EdgeMatrix();
            _finished = new EdgeMatrix();
        }

        public static int GetWorkBufferSize(int nodeCount)
        {
            return Stack.CalcBufferSize(nodeCount * nodeCount) + 0xC * nodeCount + 2 * EdgeMatrix.GetWorkBufferSize(nodeCount);
        }

        public void Initialize(Memory<byte> nodeStatesWorkBuffer, int nodeCount)
        {
            int workBufferSize = GetWorkBufferSize(nodeCount);

            Debug.Assert(nodeStatesWorkBuffer.Length >= workBufferSize);

            _nodeCount = nodeCount;

            int edgeMatrixWorkBufferSize = EdgeMatrix.GetWorkBufferSize(nodeCount);

            _discovered.Initialize(nodeStatesWorkBuffer.Slice(0, edgeMatrixWorkBufferSize), nodeCount);
            _finished.Initialize(nodeStatesWorkBuffer.Slice(edgeMatrixWorkBufferSize, edgeMatrixWorkBufferSize), nodeCount);

            nodeStatesWorkBuffer = nodeStatesWorkBuffer.Slice(edgeMatrixWorkBufferSize * 2);

            _resultArray = SpanMemoryManager<int>.Cast(nodeStatesWorkBuffer.Slice(0, sizeof(int) * nodeCount));

            nodeStatesWorkBuffer = nodeStatesWorkBuffer.Slice(sizeof(int) * nodeCount);

            Memory<int> stackWorkBuffer = SpanMemoryManager<int>.Cast(nodeStatesWorkBuffer.Slice(0, Stack.CalcBufferSize(nodeCount * nodeCount)));

            _stack.Reset(stackWorkBuffer, nodeCount * nodeCount);
        }

        private void Reset()
        {
            _discovered.Reset();
            _finished.Reset();
            _tsortResultIndex = 0;
            _resultArray.Span.Fill(-1);
        }

        private NodeState GetState(int index)
        {
            Debug.Assert(index < _nodeCount);

            if (_discovered.Test(index))
            {
                Debug.Assert(!_finished.Test(index));

                return NodeState.Discovered;
            }
            else if (_finished.Test(index))
            {
                Debug.Assert(!_discovered.Test(index));

                return NodeState.Finished;
            }

            return NodeState.Unknown;
        }

        private void SetState(int index, NodeState state)
        {
            switch (state)
            {
                case NodeState.Unknown:
                    _discovered.Reset(index);
                    _finished.Reset(index);
                    break;
                case NodeState.Discovered:
                    _discovered.Set(index);
                    _finished.Reset(index);
                    break;
                case NodeState.Finished:
                    _finished.Set(index);
                    _discovered.Reset(index);
                    break;
            }
        }

        private void PushTsortResult(int index)
        {
            Debug.Assert(index < _nodeCount);

            _resultArray.Span[_tsortResultIndex++] = index;
        }

        public ReadOnlySpan<int> GetTsortResult()
        {
            return _resultArray.Span.Slice(0, _tsortResultIndex);
        }

        public bool Sort(EdgeMatrix edgeMatrix)
        {
            Reset();

            if (_nodeCount <= 0)
            {
                return true;
            }

            for (int i = 0; i < _nodeCount; i++)
            {
                if (GetState(i) == NodeState.Unknown)
                {
                    _stack.Push(i);
                }

                while (_stack.GetCurrentCount() > 0)
                {
                    int topIndex = _stack.Top();

                    NodeState topState = GetState(topIndex);

                    if (topState == NodeState.Discovered)
                    {
                        SetState(topIndex, NodeState.Finished);
                        PushTsortResult(topIndex);
                        _stack.Pop();
                    }
                    else if (topState == NodeState.Finished)
                    {
                        _stack.Pop();
                    }
                    else
                    {
                        if (topState == NodeState.Unknown)
                        {
                            SetState(topIndex, NodeState.Discovered);
                        }

                        for (int j = 0; j < edgeMatrix.GetNodeCount(); j++)
                        {
                            if (edgeMatrix.Connected(topIndex, j))
                            {
                                NodeState jState = GetState(j);

                                if (jState == NodeState.Unknown)
                                {
                                    _stack.Push(j);
                                }
                                // Found a loop, reset and propagate rejection.
                                else if (jState == NodeState.Discovered)
                                {
                                    Reset();

                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
