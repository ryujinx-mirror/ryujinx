using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ARMeilleure.Translation
{
    static class DelegateHelper
    {
        private const string DelegateTypesAssemblyName = "JitDelegateTypes";

        private static readonly ModuleBuilder _modBuilder;

        private static readonly Dictionary<string, Type> _delegateTypesCache;

        static DelegateHelper()
        {
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(DelegateTypesAssemblyName), AssemblyBuilderAccess.Run);

            _modBuilder = asmBuilder.DefineDynamicModule(DelegateTypesAssemblyName);

            _delegateTypesCache = new Dictionary<string, Type>();
        }

        public static Delegate GetDelegate(MethodInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            Type[] parameters = info.GetParameters().Select(pI => pI.ParameterType).ToArray();
            Type   returnType = info.ReturnType;

            Type delegateType = GetDelegateType(parameters, returnType);

            return Delegate.CreateDelegate(delegateType, info);
        }

        private static Type GetDelegateType(Type[] parameters, Type returnType)
        {
            string key = GetFunctionSignatureKey(parameters, returnType);

            if (!_delegateTypesCache.TryGetValue(key, out Type delegateType))
            {
                delegateType = MakeDelegateType(parameters, returnType, key);

                _delegateTypesCache.TryAdd(key, delegateType);
            }

            return delegateType;
        }

        private static string GetFunctionSignatureKey(Type[] parameters, Type returnType)
        {
            string sig = GetTypeName(returnType);

            foreach (Type type in parameters)
            {
                sig += '_' + GetTypeName(type);
            }

            return sig;
        }

        private static string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", string.Empty);
        }

        private const MethodAttributes CtorAttributes =
            MethodAttributes.RTSpecialName |
            MethodAttributes.HideBySig     |
            MethodAttributes.Public;

        private const TypeAttributes DelegateTypeAttributes =
            TypeAttributes.Class     |
            TypeAttributes.Public    |
            TypeAttributes.Sealed    |
            TypeAttributes.AnsiClass |
            TypeAttributes.AutoClass;

        private const MethodImplAttributes ImplAttributes =
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed;

        private const MethodAttributes InvokeAttributes =
            MethodAttributes.Public    |
            MethodAttributes.HideBySig |
            MethodAttributes.NewSlot   |
            MethodAttributes.Virtual;

        private static readonly Type[] _delegateCtorSignature = { typeof(object), typeof(IntPtr) };

        private static Type MakeDelegateType(Type[] parameters, Type returnType, string name)
        {
            TypeBuilder builder = _modBuilder.DefineType(name, DelegateTypeAttributes, typeof(MulticastDelegate));

            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _delegateCtorSignature).SetImplementationFlags(ImplAttributes);

            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);

            return builder.CreateTypeInfo();
        }
    }
}
