using LibRyujinx.Jni.Identifiers;
using LibRyujinx.Jni.Pointers;
using LibRyujinx.Jni.Primitives;
using LibRyujinx.Jni.References;
using LibRyujinx.Jni.Values;
using System;

using Rxmxnx.PInvoke;

namespace LibRyujinx.Jni;

internal delegate Int32 GetVersionDelegate(JEnvRef env);

internal delegate JResult RegisterNativesDelegate(JEnvRef env, JClassLocalRef jClass,
	ReadOnlyValPtr<JNativeMethod> methods0, Int32 nMethods);

internal delegate JResult UnregisterNativesDelegate(JEnvRef env, JClassLocalRef jClass);
internal delegate JResult MonitorEnterDelegate(JEnvRef env, JObjectLocalRef jClass);
internal delegate JResult MonitorExitDelegate(JEnvRef env, JObjectLocalRef jClass);
internal delegate JResult GetVirtualMachineDelegate(JEnvRef env, out JavaVMRef jvm);
internal delegate JResult DestroyVirtualMachineDelegate(JavaVMRef vm);
internal delegate JResult AttachCurrentThreadDelegate(JavaVMRef vm, out JEnvRef env, in JavaVMAttachArgs args);
internal delegate JResult DetachCurrentThreadDelegate(JavaVMRef vm);
internal delegate JResult GetEnvDelegate(JavaVMRef vm, out JEnvRef env, Int32 version);
internal delegate JResult AttachCurrentThreadAsDaemonDelegate(JavaVMRef vm, out JEnvRef env, in JavaVMAttachArgs args);

internal delegate JResult GetCreatedVirtualMachinesDelegate(ValPtr<JavaVMRef> buffer0, Int32 bufferLength,
	out Int32 totalVms);

internal delegate JStringLocalRef NewStringDelegate(JEnvRef env, ReadOnlyValPtr<Char> chars0, Int32 length);
internal delegate Int32 GetStringLengthDelegate(JEnvRef env, JStringLocalRef jString);

internal delegate ReadOnlyValPtr<Char>
	GetStringCharsDelegate(JEnvRef env, JStringLocalRef jString, out JBoolean isCopy);

internal delegate void ReleaseStringCharsDelegate(JEnvRef env, JStringLocalRef jString, ReadOnlyValPtr<Char> chars0);
internal delegate JStringLocalRef NewStringUtfDelegate(JEnvRef env, ReadOnlyValPtr<Byte> utf8Chars0);
internal delegate Int32 GetStringUtfLengthDelegate(JEnvRef env, JStringLocalRef jString);

internal delegate ReadOnlyValPtr<Byte> GetStringUtfCharsDelegate(JEnvRef env, JStringLocalRef jString,
	out JBoolean isCopy);

internal delegate void ReleaseStringUtfCharsDelegate(JEnvRef env, JStringLocalRef jString,
	ReadOnlyValPtr<Byte> utf8Chars0);

internal delegate void GetStringRegionDelegate(JEnvRef env, JStringLocalRef jString, Int32 startIndex, Int32 length,
	ValPtr<Char> buffer0);

internal delegate void GetStringUtfRegionDelegate(JEnvRef env, JStringLocalRef jString, Int32 startIndex, Int32 length,
	ValPtr<Byte> buffer0);

internal delegate ReadOnlyValPtr<Char> GetStringCriticalDelegate(JEnvRef env, JStringLocalRef jString,
	out JBoolean isCopy);

internal delegate void ReleaseStringCriticalDelegate(JEnvRef env, JStringLocalRef jString, ReadOnlyValPtr<Char> chars0);

internal delegate JObjectLocalRef CallStaticObjectMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JBoolean CallStaticBooleanMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JByte CallStaticByteMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JChar CallStaticCharMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JShort CallStaticShortMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JInt CallStaticIntMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JLong CallStaticLongMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JFloat CallStaticFloatMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JDouble CallStaticDoubleMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate void CallStaticVoidMethodADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JObjectLocalRef GetStaticObjectFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JBoolean GetStaticBooleanFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JByte GetStaticByteFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JChar GetStaticCharFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JShort GetStaticShortFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JInt GetStaticIntFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JLong GetStaticLongFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JFloat GetStaticFloatFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);
internal delegate JDouble GetStaticDoubleFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField);

internal delegate void SetStaticObjectFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField,
	JObjectLocalRef value);

internal delegate void SetStaticBooleanFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField,
	JBoolean value);

internal delegate void SetStaticByteFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JByte value);
internal delegate void SetStaticCharFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JChar value);
internal delegate void SetStaticShortFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JShort value);
internal delegate void SetStaticIntFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JInt value);
internal delegate void SetStaticLongFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JLong value);
internal delegate void SetStaticFloatFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JFloat value);
internal delegate void SetStaticDoubleFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId jField, JDouble value);
internal delegate JMethodId FromReflectedMethodDelegate(JEnvRef env, JObjectLocalRef method);
internal delegate JFieldId FromReflectedFieldDelegate(JEnvRef env, JObjectLocalRef field);
internal delegate JResult PushLocalFrameDelegate(JEnvRef env, Int32 capacity);
internal delegate JObjectLocalRef PopLocalFrameDelegate(JEnvRef env, JObjectLocalRef result);
internal delegate JGlobalRef NewGlobalRefDelegate(JEnvRef env, JObjectLocalRef localRef);
internal delegate void DeleteGlobalRefDelegate(JEnvRef env, JGlobalRef globalRef);
internal delegate void DeleteLocalRefDelegate(JEnvRef env, JObjectLocalRef localRef);
internal delegate JBoolean IsSameObjectDelegate(JEnvRef env, JObjectLocalRef obj1, JObjectLocalRef obj2);
internal delegate JObjectLocalRef NewLocalRefDelegate(JEnvRef env, JObjectLocalRef objRef);
internal delegate JResult EnsureLocalCapacityDelegate(JEnvRef env, Int32 capacity);
internal delegate JWeakRef NewWeakGlobalRefDelegate(JEnvRef env, JObjectLocalRef obj);
internal delegate void DeleteWeakGlobalRefDelegate(JEnvRef env, JWeakRef jWeak);
internal delegate JReferenceType GetObjectRefTypeDelegate(JEnvRef env, JObjectLocalRef obj);

internal delegate JObjectLocalRef CallNonVirtualObjectMethodADelegate(JEnvRef env, JObjectLocalRef obj,
	JClassLocalRef jClass, JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JBoolean CallNonVirtualBooleanMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JByte CallNonVirtualByteMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JChar CallNonVirtualCharMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JShort CallNonVirtualShortMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JInt CallNonVirtualIntMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JLong CallNonVirtualLongMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JFloat CallNonVirtualFloatMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JDouble CallNonVirtualDoubleMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate void CallNonVirtualVoidMethodADelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass,
	JMethodId jMethod, ReadOnlyValPtr<JValue> args0);

internal delegate JObjectLocalRef CallObjectMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JBoolean CallBooleanMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JByte CallByteMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JChar CallCharMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JShort CallShortMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JInt CallIntMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JLong CallLongMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JFloat CallFloatMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JDouble CallDoubleMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate void CallVoidMethodADelegate(JEnvRef env, JObjectLocalRef obj, JMethodId jMethod,
	ReadOnlyValPtr<JValue> args0);

internal delegate JObjectLocalRef GetObjectFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JBoolean GetBooleanFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JByte GetByteFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JChar GetCharFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JShort GetShortFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JInt GetIntFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JLong GetLongFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JFloat GetFloatFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate JDouble GetDoubleFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField);
internal delegate void SetObjectFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JObjectLocalRef value);
internal delegate void SetBooleanFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JBoolean value);
internal delegate void SetByteFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JByte value);
internal delegate void SetCharFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JChar value);
internal delegate void SetShortFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JShort value);
internal delegate void SetIntFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JInt value);
internal delegate void SetLongFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JLong value);
internal delegate void SetFloatFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JFloat value);
internal delegate void SetDoubleFieldDelegate(JEnvRef env, JObjectLocalRef obj, JFieldId jField, JDouble value);
internal delegate JResult ThrowDelegate(JEnvRef env, JThrowableLocalRef obj);
internal delegate JResult ThrowNewDelegate(JEnvRef env, JClassLocalRef jClass, ReadOnlyValPtr<Byte> messageChars0);
internal delegate JThrowableLocalRef ExceptionOccurredDelegate(JEnvRef env);
internal delegate void ExceptionDescribeDelegate(JEnvRef env);
internal delegate void ExceptionClearDelegate(JEnvRef env);
internal delegate void FatalErrorDelegate(JEnvRef env, ReadOnlyValPtr<Byte> messageChars0);
internal delegate JBoolean ExceptionCheckDelegate(JEnvRef env);
internal delegate JObjectLocalRef NewDirectByteBufferDelegate(JEnvRef env, IntPtr address, Int64 capacity);
internal delegate IntPtr GetDirectBufferAddressDelegate(JEnvRef env, JObjectLocalRef buffObj);
internal delegate Int64 GetDirectBufferCapacityDelegate(JEnvRef env, JObjectLocalRef buffObj);

internal delegate JClassLocalRef DefineClassDelegate(JEnvRef env, ReadOnlyValPtr<Byte> nameChars0,
	JObjectLocalRef loader, IntPtr binaryData, Int32 len);

internal delegate JClassLocalRef FindClassDelegate(JEnvRef env, ReadOnlyValPtr<Byte> nameChars0);

internal delegate JObjectLocalRef ToReflectedMethodDelegate(JEnvRef env, JClassLocalRef jClass, JMethodId methodId,
	JBoolean isStatic);

internal delegate JClassLocalRef GetSuperclassDelegate(JEnvRef env, JClassLocalRef sub);
internal delegate JBoolean IsAssignableFromDelegate(JEnvRef env, JClassLocalRef sub, JClassLocalRef sup);
internal delegate JClassLocalRef GetObjectClassDelegate(JEnvRef env, JObjectLocalRef obj);
internal delegate JBoolean IsInstanceOfDelegate(JEnvRef env, JObjectLocalRef obj, JClassLocalRef jClass);

internal delegate JObjectLocalRef ToReflectedFieldDelegate(JEnvRef env, JClassLocalRef jClass, JFieldId fieldId,
	JBoolean isStatic);

internal delegate JMethodId GetMethodIdDelegate(JEnvRef env, JClassLocalRef jClass, ReadOnlyValPtr<Byte> nameChars0,
	ReadOnlyValPtr<Byte> signatureChars0);

internal delegate JFieldId GetFieldIdDelegate(JEnvRef env, JClassLocalRef jClass, ReadOnlyValPtr<Byte> nameChars0,
	ReadOnlyValPtr<Byte> signatureChars0);

internal delegate JMethodId GetStaticMethodIdDelegate(JEnvRef env, JClassLocalRef jClass,
	ReadOnlyValPtr<Byte> nameChars0, ReadOnlyValPtr<Byte> signatureChars0);

internal delegate JFieldId GetStaticFieldIdDelegate(JEnvRef env, JClassLocalRef jClass, ReadOnlyValPtr<Byte> nameChars0,
	ReadOnlyValPtr<Byte> signatureChars0);

internal delegate Int32 GetArrayLengthDelegate(JEnvRef env, JArrayLocalRef array);

internal delegate JArrayLocalRef NewObjectArrayDelegate(JEnvRef env, Int32 length, JClassLocalRef jClass,
	JObjectLocalRef init);

internal delegate JObjectLocalRef GetObjectArrayElementDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 index);

internal delegate void SetObjectArrayElementDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 index,
	JObjectLocalRef obj);

internal delegate JArrayLocalRef NewBooleanArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewByteArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewCharArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewShortArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewIntArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewLongArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewFloatArrayDelegate(JEnvRef env, Int32 length);
internal delegate JArrayLocalRef NewDoubleArrayDelegate(JEnvRef env, Int32 length);

internal delegate ValPtr<JBoolean> GetBooleanArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	out JBoolean isCopy);

internal delegate ValPtr<JByte> GetByteArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef, out JBoolean isCopy);
internal delegate ValPtr<JChar> GetCharArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef, out JBoolean isCopy);

internal delegate ValPtr<JShort> GetShortArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	out JBoolean isCopy);

internal delegate ValPtr<JInt> GetIntArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef, out JBoolean isCopy);
internal delegate ValPtr<JLong> GetLongArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef, out JBoolean isCopy);

internal delegate ValPtr<JFloat> GetFloatArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	out JBoolean isCopy);

internal delegate ValPtr<JDouble> GetDoubleArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	out JBoolean isCopy);

internal delegate void ReleaseBooleanArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JBoolean> elements0, JReleaseMode mode);

internal delegate void ReleaseByteArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JByte> elements0, JReleaseMode mode);

internal delegate void ReleaseCharArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JChar> elements0, JReleaseMode mode);

internal delegate void ReleaseShortArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JShort> elements0, JReleaseMode mode);

internal delegate void ReleaseIntArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JInt> elements0, JReleaseMode mode);

internal delegate void ReleaseLongArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JLong> elements0, JReleaseMode mode);

internal delegate void ReleaseFloatArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JFloat> elements0, JReleaseMode mode);

internal delegate void ReleaseDoubleArrayElementsDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ReadOnlyValPtr<JDouble> elements0, JReleaseMode mode);

internal delegate void GetBooleanArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex,
	Int32 length, ValPtr<JBoolean> buffer0);

internal delegate void GetByteArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JByte> buffer0);

internal delegate void GetCharArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JChar> buffer0);

internal delegate void GetShortArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JShort> buffer0);

internal delegate void GetIntArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JInt> buffer0);

internal delegate void GetLongArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JLong> buffer0);

internal delegate void GetFloatArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ValPtr<JFloat> buffer0);

internal delegate void GetDoubleArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex,
	Int32 length, ValPtr<JDouble> buffer0);

internal delegate void SetBooleanArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex,
	Int32 length, ReadOnlyValPtr<JBoolean> buffer0);

internal delegate void SetByteArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JByte> buffer0);

internal delegate void SetCharArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JChar> buffer0);

internal delegate void SetShortArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JShort> buffer0);

internal delegate void SetIntArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JInt> buffer0);

internal delegate void SetLongArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JLong> buffer0);

internal delegate void SetFloatArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex, Int32 length,
	ReadOnlyValPtr<JFloat> buffer0);

internal delegate void SetDoubleArrayRegionDelegate(JEnvRef env, JArrayLocalRef arrayRef, Int32 startIndex,
	Int32 length, ReadOnlyValPtr<JDouble> buffer0);

internal delegate ValPtr<Byte> GetPrimitiveArrayCriticalDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	out JBoolean isCopy);

internal delegate void ReleasePrimitiveArrayCriticalDelegate(JEnvRef env, JArrayLocalRef arrayRef,
	ValPtr<Byte> elements, JReleaseMode mode);

internal delegate JObjectLocalRef NewObjectADelegate(JEnvRef env, JClassLocalRef jClass, JMethodId jMethod,
	ReadOnlyValPtr<JValue> arg0);
