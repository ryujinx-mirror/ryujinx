using System;
using System.Diagnostics.CodeAnalysis;

namespace LibRyujinx.Jni.Identifiers
{
    public readonly struct JMethodId : IEquatable<JMethodId>
    {
#pragma warning disable 0649
        private readonly IntPtr _value;
#pragma warning restore 0649

        #region Public Methods
        public Boolean Equals(JMethodId other)
            => this._value.Equals(other._value);
        #endregion

        #region Override Methods
        public override Boolean Equals([NotNullWhen(true)] Object obj)
            => obj is JMethodId other && this.Equals(other);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion

        #region Operators
        public static Boolean operator ==(JMethodId a, JMethodId b) => a.Equals(b);
        public static Boolean operator !=(JMethodId a, JMethodId b) => !a.Equals(b);
        #endregion
    }
}
