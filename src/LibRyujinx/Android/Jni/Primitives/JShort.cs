using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JShort : IComparable<Int16>, IEquatable<Int16>, IFormattable
    {
        internal static readonly Type Type = typeof(JShort);

        public static readonly CString Signature = (CString)"S";

        private readonly Int16 _value;

        private JShort(Int16 value) => this._value = value;
        private JShort(Int32 value) => this._value = Convert.ToInt16(value);

        #region Operators
        public static implicit operator JShort(Int16 value) => new(value);
        public static implicit operator Int16(JShort jValue) => jValue._value;
        public static JShort operator +(JShort a) => a;
        public static JShort operator ++(JShort a) => new(a._value + 1);
        public static JShort operator -(JShort a) => new(-a._value);
        public static JShort operator --(JShort a) => new(a._value - 1);
        public static JShort operator +(JShort a, JShort b) => new(a._value + b._value);
        public static JShort operator +(Int16 a, JShort b) => new(a + b._value);
        public static JShort operator +(JShort a, Int16 b) => new(a._value + b);
        public static JShort operator -(JShort a, JShort b) => new(a._value - b._value);
        public static JShort operator -(Int16 a, JShort b) => new(a - b._value);
        public static JShort operator -(JShort a, Int16 b) => new(a._value - b);
        public static JShort operator *(JShort a, JShort b) => new(a._value * b._value);
        public static JShort operator *(Int16 a, JShort b) => new(a * b._value);
        public static JShort operator *(JShort a, Int16 b) => new(a._value * b);
        public static JShort operator /(JShort a, JShort b) => new(a._value / b._value);
        public static JShort operator /(Int16 a, JShort b) => new(a / b._value);
        public static JShort operator /(JShort a, Int16 b) => new(a._value / b);
        public static JShort operator %(JShort a, JShort b) => new(a._value % b._value);
        public static JShort operator %(Int16 a, JShort b) => new(a % b._value);
        public static JShort operator %(JShort a, Int16 b) => new(a._value % b);
        public static Boolean operator ==(JShort a, JShort b) => a._value.Equals(b._value);
        public static Boolean operator ==(Int16 a, JShort b) => a.Equals(b._value);
        public static Boolean operator ==(JShort a, Int16 b) => a._value.Equals(b);
        public static Boolean operator !=(JShort a, JShort b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Int16 a, JShort b) => !a.Equals(b._value);
        public static Boolean operator !=(JShort a, Int16 b) => !a._value.Equals(b);
        public static Boolean operator >(JShort a, JShort b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Int16 a, JShort b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JShort a, Int16 b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JShort a, JShort b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Int16 a, JShort b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JShort a, Int16 b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JShort a, JShort b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Int16 a, JShort b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JShort a, Int16 b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JShort a, JShort b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Int16 a, JShort b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JShort a, Int16 b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Int16 other) => this._value.CompareTo(other);
        public Int32 CompareTo(JShort other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JShort jvalue ? this.CompareTo(jvalue) : obj is Int16 value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Int16 other) => this._value.Equals(other);
        public Boolean Equals(JShort other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JShort jvalue ? this.Equals(jvalue) : obj is Int16 value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
