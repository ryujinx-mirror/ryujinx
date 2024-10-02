using LibRyujinx.Jni;
using LibRyujinx.Jni.Identifiers;
using LibRyujinx.Jni.Pointers;
using LibRyujinx.Jni.Primitives;
using LibRyujinx.Jni.References;
using LibRyujinx.Jni.Values;
using Rxmxnx.PInvoke;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Applets.SoftwareKeyboard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LibRyujinx.Android
{
    internal unsafe static class Interop
    {
        internal const string BaseClassName = "org/ryujinx/android/RyujinxNative";

        private static JGlobalRef? _classId;
        private static ConcurrentDictionary<(string method, string descriptor), JMethodId> _methodCache = new ConcurrentDictionary<(string method, string descriptor), JMethodId>();
        private static (string name, string descriptor)[] _methods = new[]
        {
            ("test", "()V"),
            ("updateUiHandler", "(JJJIIIIJJ)V"),
            ("frameEnded", "()V"),
            ("updateProgress", "(JF)V"),
            ("getSurfacePtr", "()J"),
            ("getWindowHandle", "()J")
        };

        internal static void Initialize(JEnvRef jniEnv)
        {
            var vm = JniHelper.GetVirtualMachine(jniEnv);
            if (_classId == null)
            {
                var className = new ReadOnlySpan<Byte>(Encoding.UTF8.GetBytes(BaseClassName));
                using (IReadOnlyFixedMemory<Byte>.IDisposable cName = className.GetUnsafeValPtr()
                           .GetUnsafeFixedContext(className.Length))
                {
                    _classId = JniHelper.GetGlobalClass(jniEnv, cName);
                    if (_classId == null)
                    {
                        Logger.Info?.Print(LogClass.Application, $"Class Id {BaseClassName} not found");
                        return;
                    }
                }
            }

            foreach (var x in _methods)
            {
                CacheMethod(jniEnv, x.name, x.descriptor);
            }

            JniEnv._jvm = vm;
        }

        private static void CacheMethod(JEnvRef jEnv, string name, string descriptor)
        {
            if (!_methodCache.TryGetValue((name, descriptor), out var method))
            {
                var methodName = new ReadOnlySpan<Byte>(Encoding.UTF8.GetBytes(name));
                var descriptorId = new ReadOnlySpan<Byte>(Encoding.UTF8.GetBytes(descriptor));
                using (IReadOnlyFixedMemory<Byte>.IDisposable mName = methodName.GetUnsafeValPtr()
                       .GetUnsafeFixedContext(methodName.Length))
                using (IReadOnlyFixedMemory<Byte>.IDisposable dName = descriptorId.GetUnsafeValPtr()
                       .GetUnsafeFixedContext(descriptorId.Length))
                {
                    var methodId = JniHelper.GetStaticMethodId(jEnv, (JClassLocalRef)(_classId.Value.Value), mName, dName);
                    if (methodId == null)
                    {
                        Logger.Warning?.Print(LogClass.Application, $"Java Method Id {name} not found");
                        return;
                    }

                    method = methodId.Value;
                    _methodCache[(name, descriptor)] = method;
                }
            }
        }

        private static void CallVoidMethod(string name, string descriptor, params JValue[] values)
        {
            using var env = JniEnv.Create();
            if (_methodCache.TryGetValue((name, descriptor), out var method))
            {
                if (descriptor.EndsWith("V"))
                {
                    JniHelper.CallStaticVoidMethod(env.Env, (JClassLocalRef)(_classId.Value.Value), method, values);
                }
            }
        }

        private static JLong CallLongMethod(string name, string descriptor, params JValue[] values)
        {
            using var env = JniEnv.Create();
            if (_methodCache.TryGetValue((name, descriptor), out var method))
            {
                if (descriptor.EndsWith("J"))
                    return JniHelper.CallStaticLongMethod(env.Env, (JClassLocalRef)(_classId.Value.Value), method, values) ?? (JLong)(-1);
            }

            return (JLong)(-1);
        }

        public static void Test()
        {
            CallVoidMethod("test", "()V");
        }

        public static void FrameEnded(double time)
        {
            CallVoidMethod("frameEnded", "()V");
        }

        public static void UpdateProgress(string info, float progress)
        {
            using var infoPtr = new TempNativeString(info);
            CallVoidMethod("updateProgress", "(JF)V", new JValue[]
            {
                JValue.Create(infoPtr.AsBytes()),
                JValue.Create(progress.AsBytes())
            });
        }

        public static JLong GetSurfacePtr()
        {
            return CallLongMethod("getSurfacePtr", "()J");
        }

        public static JLong GetWindowsHandle()
        {
            return CallLongMethod("getWindowHandle", "()J");
        }

        public static void UpdateUiHandler(string newTitle,
            string newMessage,
            string newWatermark,
            int newType,
            int min,
            int max,
            KeyboardMode nMode,
            string newSubtitle,
            string newInitialText)
        {
            using var titlePointer = new TempNativeString(newTitle);
            using var messagePointer = new TempNativeString(newMessage);
            using var watermarkPointer = new TempNativeString(newWatermark);
            using var subtitlePointer = new TempNativeString(newSubtitle);
            using var newInitialPointer = new TempNativeString(newInitialText);
            CallVoidMethod("updateUiHandler", "(JJJIIIIJJ)V", new JValue[]
            {
                JValue.Create(titlePointer.AsBytes()),
                JValue.Create(messagePointer.AsBytes()),
                JValue.Create(watermarkPointer.AsBytes()),
                JValue.Create(newType.AsBytes()),
                JValue.Create(min.AsBytes()),
                JValue.Create(max.AsBytes()),
                JValue.Create(nMode.AsBytes()),
                JValue.Create(subtitlePointer.AsBytes()),
                JValue.Create(newInitialPointer.AsBytes())
            });
        }

        private class TempNativeString : IDisposable
        {
            private JLong _jPointer;

            public TempNativeString(string value)
            {
                Pointer = Marshal.StringToHGlobalAuto(value);
                JPointer = (JLong)Pointer;
            }

            public nint Pointer { get; private set; }
            public JLong JPointer { get => _jPointer; private set => _jPointer = value; }

            public Span<byte> AsBytes()
            {
                return _jPointer.AsBytes();
            }

            public void Dispose()
            {
                if (Pointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Pointer);
                }
                Pointer = IntPtr.Zero;
            }
        }

        private class JniEnv : IDisposable
        {
            internal static JavaVMRef? _jvm;
            private readonly JEnvRef _env;
            private readonly bool _newAttach;

            public JEnvRef Env => _env;

            private JniEnv(JEnvRef env, bool newAttach)
            {
                _env = env;
                _newAttach = newAttach;
            }

            public void Dispose()
            {
                if(_newAttach)
                {
                    JniHelper.Detach(_jvm!.Value);
                }
            }

            public static JniEnv? Create()
            {
                bool newAttach = false;
                ReadOnlySpan<Byte> threadName = "JvmCall"u8;
                var env = _jvm == null ? default : JniHelper.Attach(_jvm.Value, threadName.GetUnsafeValPtr().GetUnsafeFixedContext(threadName.Length),
                                         out newAttach);

                return env != null ? new JniEnv(env.Value, newAttach) : null;
            }
        }
    }
}
