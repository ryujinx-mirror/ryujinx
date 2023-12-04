using System;

namespace ARMeilleure.CodeGen.Linking
{
    /// <summary>
    /// Represents a symbol.
    /// </summary>
    readonly struct Symbol
    {
        private readonly ulong _value;

        /// <summary>
        /// Gets the <see cref="SymbolType"/> of the <see cref="Symbol"/>.
        /// </summary>
        public SymbolType Type { get; }

        /// <summary>
        /// Gets the value of the <see cref="Symbol"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Type"/> is <see cref="SymbolType.None"/></exception>
        public ulong Value
        {
            get
            {
                if (Type == SymbolType.None)
                {
                    ThrowSymbolNone();
                }

                return _value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> structure with the specified <see cref="SymbolType"/> and value.
        /// </summary>
        /// <param name="type">Type of symbol</param>
        /// <param name="value">Value of symbol</param>
        public Symbol(SymbolType type, ulong value)
        {
            (Type, _value) = (type, value);
        }

        /// <summary>
        /// Determines if the specified <see cref="Symbol"/> instances are equal.
        /// </summary>
        /// <param name="a">First instance</param>
        /// <param name="b">Second instance</param>
        /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/></returns>
        public static bool operator ==(Symbol a, Symbol b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines if the specified <see cref="Symbol"/> instances are not equal.
        /// </summary>
        /// <param name="a">First instance</param>
        /// <param name="b">Second instance</param>
        /// <returns><see langword="true"/> if not equal; otherwise <see langword="false"/></returns>
        public static bool operator !=(Symbol a, Symbol b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines if the specified <see cref="Symbol"/> is equal to this <see cref="Symbol"/> instance.
        /// </summary>
        /// <param name="other">Other <see cref="Symbol"/> instance</param>
        /// <returns><see langword="true"/> if equal; otherwise <see langword="false"/></returns>
        public bool Equals(Symbol other)
        {
            return other.Type == Type && other._value == _value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Symbol sym && Equals(sym);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, _value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type}:{_value}";
        }

        private static void ThrowSymbolNone()
        {
            throw new InvalidOperationException("Symbol refers to nothing.");
        }
    }
}
