using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ARMeilleure.Translation
{
    static class DelegateCache
    {
        private static ConcurrentDictionary<string, Delegate> _delegates;

        static DelegateCache()
        {
            _delegates = new ConcurrentDictionary<string, Delegate>();
        }

        public static Delegate GetOrAdd(Delegate dlg)
        {
            return _delegates.GetOrAdd(GetKey(dlg.Method), (key) => dlg);
        }

        private static string GetKey(MethodInfo info)
        {
            return $"{info.DeclaringType.FullName}.{info.Name}";
        }
    }
}