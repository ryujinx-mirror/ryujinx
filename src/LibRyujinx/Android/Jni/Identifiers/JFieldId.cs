using System;
using System.Diagnostics.CodeAnalysis;

namespace LibRyujinx.Jni.Identifiers
{
    public readonly struct JFieldId : IEquatable<JFieldId>
    {
#pragma warning disable 0649
        private readonly IntPtr _value;
#pragma warning restore 0649

        #region Public Methods
        public Boolean Equals(JFieldId other)
            => this._value.Equals(other._value);
        #endregion

        #region Override Methods
        public override Boolean Equals([NotNullWhen(true)] Object obj)
            => obj is JFieldId other && this.Equals(other);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion

        #region Operators
        public static Boolean operator ==(JFieldId a, JFieldId b) => a.Equals(b);
        public static Boolean operator !=(JFieldId a, JFieldId b) => !a.Equals(b);
        #endregion
    }
}
