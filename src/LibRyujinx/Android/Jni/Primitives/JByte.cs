using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JByte : IComparable<SByte>, IEquatable<SByte>, IFormattable
    {
        internal static readonly Type Type = typeof(JByte);

        public static readonly CString Signature = (CString)"B";

        private readonly SByte _value;

        private JByte(SByte value) => this._value = value;
        private JByte(Int32 value) => this._value = Convert.ToSByte(value);

        #region Operators
        public static implicit operator JByte(SByte value) => new(value);
        public static implicit operator SByte(JByte jValue) => jValue._value;
        public static JByte operator +(JByte a) => a;
        public static JByte operator ++(JByte a) => new(a._value + 1);
        public static JByte operator -(JByte a) => new(-a._value);
        public static JByte operator --(JByte a) => new(a._value - 1);
        public static JByte operator +(JByte a, JByte b) => new(a._value + b._value);
        public static JByte operator +(SByte a, JByte b) => new(a + b._value);
        public static JByte operator +(JByte a, SByte b) => new(a._value + b);
        public static JByte operator -(JByte a, JByte b) => new(a._value - b._value);
        public static JByte operator -(SByte a, JByte b) => new(a - b._value);
        public static JByte operator -(JByte a, SByte b) => new(a._value - b);
        public static JByte operator *(JByte a, JByte b) => new(a._value * b._value);
        public static JByte operator *(SByte a, JByte b) => new(a * b._value);
        public static JByte operator *(JByte a, SByte b) => new(a._value * b);
        public static JByte operator /(JByte a, JByte b) => new(a._value / b._value);
        public static JByte operator /(SByte a, JByte b) => new(a / b._value);
        public static JByte operator /(JByte a, SByte b) => new(a._value / b);
        public static JByte operator %(JByte a, JByte b) => new(a._value % b._value);
        public static JByte operator %(SByte a, JByte b) => new(a % b._value);
        public static JByte operator %(JByte a, SByte b) => new(a._value % b);
        public static Boolean operator ==(JByte a, JByte b) => a._value.Equals(b._value);
        public static Boolean operator ==(SByte a, JByte b) => a.Equals(b._value);
        public static Boolean operator ==(JByte a, SByte b) => a._value.Equals(b);
        public static Boolean operator !=(JByte a, JByte b) => !a._value.Equals(b._value);
        public static Boolean operator !=(SByte a, JByte b) => !a.Equals(b._value);
        public static Boolean operator !=(JByte a, SByte b) => !a._value.Equals(b);
        public static Boolean operator >(JByte a, JByte b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(SByte a, JByte b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JByte a, SByte b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JByte a, JByte b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(SByte a, JByte b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JByte a, SByte b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JByte a, JByte b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(SByte a, JByte b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JByte a, SByte b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JByte a, JByte b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(SByte a, JByte b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JByte a, SByte b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(SByte other) => this._value.CompareTo(other);
        public Int32 CompareTo(JByte other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JByte jValue ? this.CompareTo(jValue) : obj is SByte value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(SByte other) => this._value.Equals(other);
        public Boolean Equals(JByte other) => this._value.Equals(other._value);
        public String ToString(String format, IFormatProvider formatProvider) => this._value.ToString(format, formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JByte jvalue ? this.Equals(jvalue) : obj is SByte value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
