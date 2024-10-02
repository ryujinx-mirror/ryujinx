using LibRyujinx.Jni.Values;

using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Pointers;

public readonly struct JavaVMRef : IEquatable<JavaVMRef>
{
#pragma warning disable 0649
	private readonly IntPtr _value;
#pragma warning restore 0649

	#region Operators
	public static Boolean operator ==(JavaVMRef a, JavaVMRef b) => a._value.Equals(b._value);
	public static Boolean operator !=(JavaVMRef a, JavaVMRef b) => !a._value.Equals(b._value);
	#endregion

	#region Public Properties
	internal readonly ref JavaVMValue VirtualMachine => ref this._value.GetUnsafeReference<JavaVMValue>();
	#endregion

	#region Public Methods
	public Boolean Equals(JavaVMRef other) => this._value.Equals(other._value);
	#endregion

	#region Overrided Methods
	public override Boolean Equals(Object obj) => obj is JavaVMRef other && this.Equals(other);
	public override Int32 GetHashCode() => this._value.GetHashCode();
	#endregion
}
