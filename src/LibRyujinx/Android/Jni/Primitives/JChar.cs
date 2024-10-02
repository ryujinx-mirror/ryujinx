using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Primitives
{
    public readonly struct JChar : IComparable<Char>, IEquatable<Char>
    {
        internal static readonly Type Type = typeof(JChar);

        public static readonly CString Signature = (CString)"C";

        private readonly Char _value;

        private JChar(Char value) => this._value = value;

        #region Operators
        public static implicit operator JChar(Char value) => new(value);
        public static explicit operator JChar(Int16 value) => new((Char)value);
        public static implicit operator Char(JChar jValue) => jValue._value;
        public static explicit operator Int16(JChar jValue) => (Int16)jValue._value;
        public static Boolean operator ==(JChar a, JChar b) => a._value.Equals(b._value);
        public static Boolean operator ==(Char a, JChar b) => a.Equals(b._value);
        public static Boolean operator ==(JChar a, Char b) => a._value.Equals(b);
        public static Boolean operator !=(JChar a, JChar b) => !a._value.Equals(b._value);
        public static Boolean operator !=(Char a, JChar b) => !a.Equals(b._value);
        public static Boolean operator !=(JChar a, Char b) => !a._value.Equals(b);
        public static Boolean operator >(JChar a, JChar b) => a._value.CompareTo(b._value) > 0;
        public static Boolean operator >(Char a, JChar b) => a.CompareTo(b._value) > 0;
        public static Boolean operator >(JChar a, Char b) => a._value.CompareTo(b) > 0;
        public static Boolean operator <(JChar a, JChar b) => a._value.CompareTo(b._value) < 0;
        public static Boolean operator <(Char a, JChar b) => a.CompareTo(b._value) < 0;
        public static Boolean operator <(JChar a, Char b) => a._value.CompareTo(b) < 0;
        public static Boolean operator >=(JChar a, JChar b) => a._value.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(Char a, JChar b) => a.CompareTo(b._value) > 0 || a.Equals(b._value);
        public static Boolean operator >=(JChar a, Char b) => a._value.CompareTo(b) > 0 || a._value.Equals(b);
        public static Boolean operator <=(JChar a, JChar b) => a._value.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(Char a, JChar b) => a.CompareTo(b._value) < 0 || a.Equals(b._value);
        public static Boolean operator <=(JChar a, Char b) => a._value.CompareTo(b) < 0 || a._value.Equals(b);
        #endregion

        #region Public Methods
        public Int32 CompareTo(Char other) => this._value.CompareTo(other);
        public Int32 CompareTo(JChar other) => this._value.CompareTo(other._value);
        public Int32 CompareTo(Object obj) => obj is JChar jvalue ? this.CompareTo(jvalue) : obj is Char value ? this.CompareTo(value) : this._value.CompareTo(obj);
        public Boolean Equals(Char other) => this._value.Equals(other);
        public Boolean Equals(JChar other) => this._value.Equals(other._value);
        public String ToString(IFormatProvider formatProvider) => this._value.ToString(formatProvider);
        #endregion

        #region Overrided Methods
        public override String ToString() => this._value.ToString();
        public override Boolean Equals(Object obj) => obj is JChar jvalue ? this.Equals(jvalue) : obj is Char value ? this.Equals(value) : this._value.Equals(obj);
        public override Int32 GetHashCode() => this._value.GetHashCode();
        #endregion
    }
}
