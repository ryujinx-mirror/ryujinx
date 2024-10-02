using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Values;

internal readonly struct JavaVMValue : IEquatable<JavaVMValue>
{
#pragma warning disable 0649
	private readonly IntPtr _value;
#pragma warning restore 0649

	#region Operators
	public static Boolean operator ==(JavaVMValue a, JavaVMValue b) => a._value.Equals(b._value);
	public static Boolean operator !=(JavaVMValue a, JavaVMValue b) => !a._value.Equals(b._value);
	#endregion

	#region Public Properties
	internal readonly ref JInvokeInterface Functions => ref this._value.GetUnsafeReference<JInvokeInterface>();
	#endregion

	#region Public Methods
	public Boolean Equals(JavaVMValue other) => this._value.Equals(other._value);
	#endregion

	#region Overrided Methods
	public override Boolean Equals(Object obj) => obj is JavaVMValue other && this.Equals(other);
	public override Int32 GetHashCode() => this._value.GetHashCode();
	#endregion
}
