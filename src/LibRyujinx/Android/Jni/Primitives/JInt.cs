using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JInt : IComparable<Int32>, IEquatable<Int32>, IFormattable
    {
        internal static readonly Type Type = typeof(JInt);

        public static readonly CString Signature = (CString)"I";

        private readonly Int32 _value;

        private JInt(Int32 value) => this._value = value;

        #region Operators
        public static implicit operator JInt(Int32 value) => new(value);
        public static implicit operator Int32(JInt jValue) => jValue._value;
        public static JInt operator +(JInt a) => a;
        public static JInt operator ++(JInt a) => new(a._value + 1);
        public static JInt operator -(JInt a) => new(-a._value);
        public static JInt operator --(JInt a) => new(a._value - 1);
        public static JInt operator +(JInt a, JInt b) => new(a._value + b._value);
        public static JInt operator +(Int32 a, JInt b) => new(a + b._value);
        public static JInt operator +(JInt a, Int32 b) => new(a._value + b);
        public static JInt operator -(JInt a, JInt b) => new(a._value - b._value);
        public static JInt operator -(Int32 a, JInt b) => new(a - b._value);
        public static JInt operator -(JInt a, Int32 b) => new(a._value - b);
        public static JInt operator *(JInt a, JInt b) => new(a._value * b._value);
        public static JInt operator *(Int32 a, JInt b) => new(a * b._value);
        public static JInt operator *(JInt a, Int32 b) => new(a._value * b);
        public static JInt operator /(JInt a, JInt b) => new(a._value / b._value);
        public static JInt operator /(Int32 a, JInt b) => new(a / b._value);
        public static JInt operator /(JInt a, Int32 b) => new(a._value / b);
        public static JInt operator %(JInt a, JInt b) => new(a._value % b._value);
        public static JInt operator %(Int32 a, JInt b) => new(a % b._value);
        public static JInt operator %(JInt a, Int32 b) => new(a._value % b);
        public static Boolean operator ==(JInt a, JInt b) => a._value.Equals(b._value);
        public static Boolean operator ==(Int32 a, JInt b) => a.Equals(b._value);
        public static Boolean operator ==(JInt a, Int32 b) => a._value.Equals(b);
        public static Boolean operator !=(JInt a, JInt b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Int32 a, JInt b) => !a.Equals(b._value);
        public static Boolean operator !=(JInt a, Int32 b) => !a._value.Equals(b);
        public static Boolean operator >(JInt a, JInt b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Int32 a, JInt b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JInt a, Int32 b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JInt a, JInt b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Int32 a, JInt b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JInt a, Int32 b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JInt a, JInt b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Int32 a, JInt b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JInt a, Int32 b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JInt a, JInt b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Int32 a, JInt b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JInt a, Int32 b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Int32 other) => this._value.CompareTo(other);
        public Int32 CompareTo(JInt other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JInt jValue ? this.CompareTo(jValue) : obj is Int32 value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Int32 other) => this._value.Equals(other);
        public Boolean Equals(JInt other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JInt jvalue ? this.Equals(jvalue) : obj is Int32 value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
