using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JDouble : IComparable<Double>, IEquatable<Double>, IFormattable
    {
        internal static readonly Type Type = typeof(JDouble);

        public static readonly CString Signature = (CString)"D";

        private readonly Double _value;

        private JDouble(Double value) => this._value = value;

        #region Operators
        public static implicit operator JDouble(Double value) => new(value);
        public static implicit operator Double(JDouble jValue) => jValue._value;
        public static JDouble operator +(JDouble a) => a;
        public static JDouble operator ++(JDouble a) => new(a._value + 1);
        public static JDouble operator -(JDouble a) => new(-a._value);
        public static JDouble operator --(JDouble a) => new(a._value - 1);
        public static JDouble operator +(JDouble a, JDouble b) => new(a._value + b._value);
        public static JDouble operator +(Double a, JDouble b) => new(a + b._value);
        public static JDouble operator +(JDouble a, Double b) => new(a._value + b);
        public static JDouble operator -(JDouble a, JDouble b) => new(a._value - b._value);
        public static JDouble operator -(Double a, JDouble b) => new(a - b._value);
        public static JDouble operator -(JDouble a, Double b) => new(a._value - b);
        public static JDouble operator *(JDouble a, JDouble b) => new(a._value * b._value);
        public static JDouble operator *(Double a, JDouble b) => new(a * b._value);
        public static JDouble operator *(JDouble a, Double b) => new(a._value * b);
        public static JDouble operator /(JDouble a, JDouble b) => new(a._value / b._value);
        public static JDouble operator /(Double a, JDouble b) => new(a / b._value);
        public static JDouble operator /(JDouble a, Double b) => new(a._value / b);
        public static Boolean operator ==(JDouble a, JDouble b) => a._value.Equals(b._value);
        public static Boolean operator ==(Double a, JDouble b) => a.Equals(b._value);
        public static Boolean operator ==(JDouble a, Double b) => a._value.Equals(b);
        public static Boolean operator !=(JDouble a, JDouble b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Double a, JDouble b) => !a.Equals(b._value);
        public static Boolean operator !=(JDouble a, Double b) => !a._value.Equals(b);
        public static Boolean operator >(JDouble a, JDouble b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Double a, JDouble b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JDouble a, Double b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JDouble a, JDouble b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Double a, JDouble b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JDouble a, Double b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JDouble a, JDouble b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Double a, JDouble b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JDouble a, Double b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JDouble a, JDouble b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Double a, JDouble b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JDouble a, Double b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Double other) => this._value.CompareTo(other);
        public Int32 CompareTo(JDouble other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JDouble jvalue ? this.CompareTo(jvalue) : obj is Double value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Double other) => this._value.Equals(other);
        public Boolean Equals(JDouble other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JDouble jvalue ? this.Equals(jvalue) : obj is Double value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
