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

using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Effect context.
    /// </summary>
    public class EffectContext
    {
        /// <summary>
        /// Storage for <see cref="BaseEffect"/>.
        /// </summary>
        private BaseEffect[] _effects;

        /// <summary>
        /// The total effect count.
        /// </summary>
        private uint _effectCount;

        /// <summary>
        /// Create a new <see cref="EffectContext"/>.
        /// </summary>
        public EffectContext()
        {
            _effects = null;
            _effectCount = 0;
        }

        /// <summary>
        /// Initialize the <see cref="EffectContext"/>.
        /// </summary>
        /// <param name="effectCount">The total effect count.</param>
        public void Initialize(uint effectCount)
        {
            _effectCount = effectCount;
            _effects = new BaseEffect[effectCount];

            for (int i = 0; i < _effectCount; i++)
            {
                _effects[i] = new BaseEffect();
            }
        }

        /// <summary>
        /// Get the total effect count.
        /// </summary>
        /// <returns>The total effect count.</returns>
        public uint GetCount()
        {
            return _effectCount;
        }

        /// <summary>
        /// Get a reference to a <see cref="BaseEffect"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>A reference to a <see cref="BaseEffect"/> at the given <paramref name="index"/>.</returns>
        public ref BaseEffect GetEffect(int index)
        {
            Debug.Assert(index >= 0 && index < _effectCount);

            return ref _effects[index];
        }
    }
}
