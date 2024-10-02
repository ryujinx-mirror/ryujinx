using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JLong : IComparable<Int64>, IEquatable<Int64>, IFormattable
    {
        internal static readonly Type Type = typeof(JLong);

        public static readonly CString Signature = (CString)"J";

        private readonly Int64 _value;

        private JLong(Int64 value) => this._value = value;

        #region Operators
        public static implicit operator JLong(Int64 value) => new(value);
        public static implicit operator Int64(JLong jValue) => jValue._value;
        public static JLong operator +(JLong a) => a;
        public static JLong operator ++(JLong a) => new(a._value + 1);
        public static JLong operator -(JLong a) => new(-a._value);
        public static JLong operator --(JLong a) => new(a._value - 1);
        public static JLong operator +(JLong a, JLong b) => new(a._value + b._value);
        public static JLong operator +(Int64 a, JLong b) => new(a + b._value);
        public static JLong operator +(JLong a, Int64 b) => new(a._value + b);
        public static JLong operator -(JLong a, JLong b) => new(a._value - b._value);
        public static JLong operator -(Int64 a, JLong b) => new(a - b._value);
        public static JLong operator -(JLong a, Int64 b) => new(a._value - b);
        public static JLong operator *(JLong a, JLong b) => new(a._value * b._value);
        public static JLong operator *(Int64 a, JLong b) => new(a * b._value);
        public static JLong operator *(JLong a, Int64 b) => new(a._value * b);
        public static JLong operator /(JLong a, JLong b) => new(a._value / b._value);
        public static JLong operator /(Int64 a, JLong b) => new(a / b._value);
        public static JLong operator /(JLong a, Int64 b) => new(a._value / b);
        public static JLong operator %(JLong a, JLong b) => new(a._value % b._value);
        public static JLong operator %(Int64 a, JLong b) => new(a % b._value);
        public static JLong operator %(JLong a, Int64 b) => new(a._value % b);
        public static Boolean operator ==(JLong a, JLong b) => a._value.Equals(b._value);
        public static Boolean operator ==(Int64 a, JLong b) => a.Equals(b._value);
        public static Boolean operator ==(JLong a, Int64 b) => a._value.Equals(b);
        public static Boolean operator !=(JLong a, JLong b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Int64 a, JLong b) => !a.Equals(b._value);
        public static Boolean operator !=(JLong a, Int64 b) => !a._value.Equals(b);
        public static Boolean operator >(JLong a, JLong b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Int64 a, JLong b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JLong a, Int64 b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JLong a, JLong b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Int64 a, JLong b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JLong a, Int64 b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JLong a, JLong b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Int64 a, JLong b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JLong a, Int64 b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JLong a, JLong b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Int64 a, JLong b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JLong a, Int64 b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Int64 other) => this._value.CompareTo(other);
        public Int32 CompareTo(JLong other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JLong jvalue ? this.CompareTo(jvalue) : obj is Int64 value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Int64 other) => this._value.Equals(other);
        public Boolean Equals(JLong other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JLong jvalue ? this.Equals(jvalue) : obj is Int64 value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
