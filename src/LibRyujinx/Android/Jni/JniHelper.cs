using LibRyujinx.Jni.Identifiers;
using LibRyujinx.Jni.Pointers;
using LibRyujinx.Jni.Primitives;
using LibRyujinx.Jni.References;
using LibRyujinx.Jni.Values;
using System;

using Rxmxnx.PInvoke;

namespace LibRyujinx.Jni
{
    internal static class JniHelper
    {
        public const Int32 JniVersion = 0x00010006; //JNI_VERSION_1_6;

        public static JEnvRef? Attach(JavaVMRef javaVm, IReadOnlyFixedMemory<Byte> threadName, out Boolean newAttach)
        {
            ref JavaVMValue value = ref javaVm.VirtualMachine;
            ref JInvokeInterface jInvoke = ref value.Functions;

            IntPtr getEnvPtr = jInvoke.GetEnvPointer;
            GetEnvDelegate getEnv = getEnvPtr.GetUnsafeDelegate<GetEnvDelegate>()!;

            if (getEnv(javaVm, out JEnvRef jEnv, JniHelper.JniVersion) == JResult.Ok)
            {
                newAttach = false;
                return jEnv;
            }

            JavaVMAttachArgs args = new() { Version = JniHelper.JniVersion, Name = threadName.ValuePointer, };
            IntPtr attachCurrentThreadPtr = jInvoke.AttachCurrentThreadPointer;
            AttachCurrentThreadDelegate attachCurrentThread =
                attachCurrentThreadPtr.GetUnsafeDelegate<AttachCurrentThreadDelegate>()!;

            newAttach = true;
            return attachCurrentThread(javaVm, out jEnv, in args) == JResult.Ok ? jEnv : null;
        }
        public static JEnvRef? AttachDaemon(JavaVMRef javaVm, IReadOnlyFixedMemory<Byte> daemonName)
        {
            ref JavaVMValue value = ref javaVm.VirtualMachine;
            ref JInvokeInterface jInvoke = ref value.Functions;

            JavaVMAttachArgs args = new() { Version = JniHelper.JniVersion, Name = daemonName.ValuePointer, };
            IntPtr attachCurrentThreadAsDaemonPtr = jInvoke.AttachCurrentThreadAsDaemonPointer;
            AttachCurrentThreadAsDaemonDelegate attachCurrentThreadAsDaemon =
                attachCurrentThreadAsDaemonPtr.GetUnsafeDelegate<AttachCurrentThreadAsDaemonDelegate>()!;

            return attachCurrentThreadAsDaemon(javaVm, out JEnvRef jEnv, in args) == JResult.Ok ? jEnv : null;
        }
        public static void Detach(JavaVMRef javaVm)
        {
            ref JavaVMValue value = ref javaVm.VirtualMachine;
            ref JInvokeInterface jInvoke = ref value.Functions;

            IntPtr detachCurrentThreadPtr = jInvoke.DetachCurrentThreadPointer;
            DetachCurrentThreadDelegate detachCurrentThread =
                detachCurrentThreadPtr.GetUnsafeDelegate<DetachCurrentThreadDelegate>()!;

            detachCurrentThread(javaVm);
        }
        public static JGlobalRef? GetGlobalClass(JEnvRef jEnv, IReadOnlyFixedMemory<Byte> className)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr findClassPtr = jInterface.FindClassPointer;
            FindClassDelegate findClass = findClassPtr.GetUnsafeDelegate<FindClassDelegate>()!;
            JClassLocalRef jClass = findClass(jEnv, className.ValuePointer);

            if (JniHelper.ExceptionCheck(jEnv))
                return default;

            IntPtr newGlobalRefPtr = jInterface.NewGlobalRefPointer;
            NewGlobalRefDelegate newGlobalRef = newGlobalRefPtr.GetUnsafeDelegate<NewGlobalRefDelegate>()!;

            JGlobalRef jGlobal = newGlobalRef(jEnv, (JObjectLocalRef)jClass);
            JniHelper.RemoveLocal(jEnv, (JObjectLocalRef)jClass);

            return !JniHelper.ExceptionCheck(jEnv) ? jGlobal : null;
        }
        public static void RemoveLocal(JEnvRef jEnv, JObjectLocalRef jObject)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr deleteLocalRefPtr = jInterface.DeleteLocalRefPointer;
            DeleteLocalRefDelegate deleteLocalRef = deleteLocalRefPtr.GetUnsafeDelegate<DeleteLocalRefDelegate>()!;

            deleteLocalRef(jEnv, jObject);
        }
        public static void RemoveGlobal(JEnvRef jEnv, JGlobalRef jObject)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr deleteGlobalRefPtr = jInterface.DeleteGlobalRefPointer;
            DeleteGlobalRefDelegate deleteGlobalRef = deleteGlobalRefPtr.GetUnsafeDelegate<DeleteGlobalRefDelegate>()!;

            deleteGlobalRef(jEnv, jObject);
        }
        public static void RemoveWeakGlobal(JEnvRef jEnv, JWeakRef jObject)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr deleteWeakGlobalRefPtr = jInterface.DeleteWeakGlobalRefPointer;
            DeleteWeakGlobalRefDelegate deleteWeakGlobalRef =
                deleteWeakGlobalRefPtr.GetUnsafeDelegate<DeleteWeakGlobalRefDelegate>()!;

            deleteWeakGlobalRef(jEnv, jObject);
        }
        public static JStringLocalRef? CreateString(JEnvRef jEnv, String textValue)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr newStringPtr = jInterface.NewStringPointer;
            NewStringDelegate newString = newStringPtr.GetUnsafeDelegate<NewStringDelegate>()!;
            using IReadOnlyFixedMemory<Char>.IDisposable ctx = textValue.AsMemory().GetFixedContext();
            JStringLocalRef jString = newString(jEnv, ctx.ValuePointer, ctx.Values.Length);

            return !JniHelper.ExceptionCheck(jEnv) ? jString : null;
        }
        public static JWeakRef? CreateWeakGlobal(JEnvRef jEnv, JObjectLocalRef jObject)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr newWeakGlobalRefPtr = jInterface.NewWeakGlobalRefPointer;
            NewWeakGlobalRefDelegate newWeakGlobalRef = newWeakGlobalRefPtr.GetUnsafeDelegate<NewWeakGlobalRefDelegate>()!;
            JWeakRef jWeak = newWeakGlobalRef(jEnv, jObject);

            return !JniHelper.ExceptionCheck(jEnv) ? jWeak : null;
        }
        private static Boolean ExceptionCheck(JEnvRef jEnv)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr exceptionCheckPtr = jInterface.ExceptionCheckPointer;
            ExceptionCheckDelegate exceptionCheck = exceptionCheckPtr.GetUnsafeDelegate<ExceptionCheckDelegate>()!;

            if (!exceptionCheck(jEnv))
                return false;
            IntPtr exceptionDescribePtr = jInterface.ExceptionDescribePointer;
            IntPtr exceptionClearPtr = jInterface.ExceptionClearPointer;

            ExceptionDescribeDelegate exceptionDescribe =
                exceptionDescribePtr.GetUnsafeDelegate<ExceptionDescribeDelegate>()!;
            ExceptionClearDelegate exceptionClear = exceptionClearPtr.GetUnsafeDelegate<ExceptionClearDelegate>()!;

            exceptionDescribe(jEnv);
            exceptionClear(jEnv);
            return true;
        }
        public static JavaVMRef? GetVirtualMachine(JEnvRef jEnv)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr getJavaVmPtr = jInterface.GetJavaVMPointer;
            GetVirtualMachineDelegate getJavaVm = getJavaVmPtr.GetUnsafeDelegate<GetVirtualMachineDelegate>()!;
            return getJavaVm(jEnv, out JavaVMRef javaVm) == JResult.Ok ? javaVm : null;
        }
        public static Boolean? IsValidGlobalWeak(JEnvRef jEnv, JWeakRef jWeak)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr isSameObjectPtr = jInterface.IsSameObjectPointer;
            IsSameObjectDelegate isSameObject = isSameObjectPtr.GetUnsafeDelegate<IsSameObjectDelegate>()!;
            JBoolean result = isSameObject(jEnv, (JObjectLocalRef)jWeak, default);
            return !JniHelper.ExceptionCheck(jEnv) ? !result : null;
        }
        public static JMethodId? GetMethodId(JEnvRef jEnv, JClassLocalRef jClass, IReadOnlyFixedMemory<Byte> methodName,
            IReadOnlyFixedMemory<Byte> descriptor)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr getMethodIdPtr = jInterface.GetMethodIdPointer;
            GetMethodIdDelegate getMethodId = getMethodIdPtr.GetUnsafeDelegate<GetMethodIdDelegate>()!;
            JMethodId methodId = getMethodId(jEnv, jClass, methodName.ValuePointer, descriptor.ValuePointer);
            return !JniHelper.ExceptionCheck(jEnv) ? methodId : null;
        }
        public static JMethodId? GetStaticMethodId(JEnvRef jEnv, JClassLocalRef jClass,
            IReadOnlyFixedMemory<Byte> methodName, IReadOnlyFixedMemory<Byte> descriptor)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr getStaticMethodIdPtr = jInterface.GetStaticMethodIdPointer;
            GetStaticMethodIdDelegate getStaticMethodId =
                getStaticMethodIdPtr.GetUnsafeDelegate<GetStaticMethodIdDelegate>()!;
            JMethodId jMethodId = getStaticMethodId(jEnv, jClass, methodName.ValuePointer, descriptor.ValuePointer);
            return !JniHelper.ExceptionCheck(jEnv) ? jMethodId : null;
        }
        public static void CallStaticVoidMethod(JEnvRef jEnv, JClassLocalRef jClass, JMethodId jMethodId,
            params JValue[] args)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr callStaticVoidMethodPtr = jInterface.CallStaticVoidMethodAPointer;
            CallStaticVoidMethodADelegate callStaticVoidMethod =
                callStaticVoidMethodPtr.GetUnsafeDelegate<CallStaticVoidMethodADelegate>()!;

            using IReadOnlyFixedMemory<JValue>.IDisposable fArgs = args.AsMemory().GetFixedContext();
            callStaticVoidMethod(jEnv, jClass, jMethodId, fArgs.ValuePointer);
        }
        public static JObjectLocalRef? CallStaticObjectMethod(JEnvRef jEnv, JClassLocalRef jClass, JMethodId jMethodId,
            params JValue[] args)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr callStaticObjectMethodPtr = jInterface.CallStaticObjectMethodAPointer;
            CallStaticObjectMethodADelegate callStaticObjectMethod =
                callStaticObjectMethodPtr.GetUnsafeDelegate<CallStaticObjectMethodADelegate>()!;

            using IReadOnlyFixedMemory<JValue>.IDisposable fArgs = args.AsMemory().GetFixedContext();
            JObjectLocalRef jObject = callStaticObjectMethod(jEnv, jClass, jMethodId, fArgs.ValuePointer);
            return !JniHelper.ExceptionCheck(jEnv) ? jObject : null;
        }
        public static JLong? CallStaticLongMethod(JEnvRef jEnv, JClassLocalRef jClass, JMethodId jMethodId,
            params JValue[] args)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr callStaticLongMethodPtr = jInterface.CallStaticLongMethodAPointer;
            CallStaticLongMethodADelegate callStaticLongMethod =
                callStaticLongMethodPtr.GetUnsafeDelegate<CallStaticLongMethodADelegate>()!;

            using IReadOnlyFixedMemory<JValue>.IDisposable fArgs = args.AsMemory().GetFixedContext();
            JLong jLong = callStaticLongMethod(jEnv, jClass, jMethodId, fArgs.ValuePointer);
            return !JniHelper.ExceptionCheck(jEnv) ? jLong : null;
        }
        public static void CallVoidMethod(JEnvRef jEnv, JObjectLocalRef jObject, JMethodId jMethodId, params JValue[] args)
        {
            ref readonly JEnvValue value = ref jEnv.Environment;
            ref JNativeInterface jInterface = ref value.Functions;

            IntPtr callVoidMethodPtr = jInterface.CallVoidMethodAPointer;
            CallVoidMethodADelegate callVoidMethod = callVoidMethodPtr.GetUnsafeDelegate<CallVoidMethodADelegate>()!;

            using IReadOnlyFixedMemory<JValue>.IDisposable fArgs = args.AsMemory().GetFixedContext();
            callVoidMethod(jEnv, jObject, jMethodId, fArgs.ValuePointer);
        }
    }
}
