using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JFloat : IComparable<Single>, IEquatable<Single>, IFormattable
    {
        internal static readonly Type Type = typeof(JFloat);

        public static readonly CString Signature = (CString)"F";

        private readonly Single _value;

        private JFloat(Single value) => this._value = value;

        #region Operators
        public static implicit operator JFloat(Single value) => new(value);
        public static implicit operator Single(JFloat jValue) => jValue._value;
        public static JFloat operator +(JFloat a) => a;
        public static JFloat operator ++(JFloat a) => new(a._value + 1);
        public static JFloat operator -(JFloat a) => new(-a._value);
        public static JFloat operator --(JFloat a) => new(a._value - 1);
        public static JFloat operator +(JFloat a, JFloat b) => new(a._value + b._value);
        public static JFloat operator +(Single a, JFloat b) => new(a + b._value);
        public static JFloat operator +(JFloat a, Single b) => new(a._value + b);
        public static JFloat operator -(JFloat a, JFloat b) => new(a._value - b._value);
        public static JFloat operator -(Single a, JFloat b) => new(a - b._value);
        public static JFloat operator -(JFloat a, Single b) => new(a._value - b);
        public static JFloat operator *(JFloat a, JFloat b) => new(a._value * b._value);
        public static JFloat operator *(Single a, JFloat b) => new(a * b._value);
        public static JFloat operator *(JFloat a, Single b) => new(a._value * b);
        public static JFloat operator /(JFloat a, JFloat b) => new(a._value / b._value);
        public static JFloat operator /(Single a, JFloat b) => new(a / b._value);
        public static JFloat operator /(JFloat a, Single b) => new(a._value / b);
        public static Boolean operator ==(JFloat a, JFloat b) => a._value.Equals(b._value);
        public static Boolean operator ==(Single a, JFloat b) => a.Equals(b._value);
        public static Boolean operator ==(JFloat a, Single b) => a._value.Equals(b);
        public static Boolean operator !=(JFloat a, JFloat b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Single a, JFloat b) => !a.Equals(b._value);
        public static Boolean operator !=(JFloat a, Single b) => !a._value.Equals(b);
        public static Boolean operator >(JFloat a, JFloat b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Single a, JFloat b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JFloat a, Single b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JFloat a, JFloat b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Single a, JFloat b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JFloat a, Single b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JFloat a, JFloat b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Single a, JFloat b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JFloat a, Single b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JFloat a, JFloat b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Single a, JFloat b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JFloat a, Single b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Single other) => this._value.CompareTo(other);
        public Int32 CompareTo(JFloat other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JFloat jvalue ? this.CompareTo(jvalue) : obj is Single value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Single other) => this._value.Equals(other);
        public Boolean Equals(JFloat other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JFloat jvalue ? this.Equals(jvalue) : obj is Single value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
