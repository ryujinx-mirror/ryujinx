using System;
using System.Diagnostics.CodeAnalysis;

namespace LibRyujinx.Jni.References;

public readonly struct JArrayLocalRef : IEquatable<JArrayLocalRef>
{
#pragma warning disable 0649
	private readonly JObjectLocalRef _value;
#pragma warning restore 0649

	#region Public Methods
	public Boolean Equals(JArrayLocalRef other) => this._value.Equals(other._value);
	#endregion

	#region Override Methods
	public override Boolean Equals([NotNullWhen(true)] Object obj) => obj is JArrayLocalRef other && this.Equals(other);
	public override Int32 GetHashCode() => this._value.GetHashCode();
	#endregion

	#region Operators
	public static explicit operator JObjectLocalRef(JArrayLocalRef a) => a._value;
	public static Boolean operator ==(JArrayLocalRef a, JArrayLocalRef b) => a.Equals(b);
	public static Boolean operator !=(JArrayLocalRef a, JArrayLocalRef b) => !a.Equals(b);
	#endregion
}
