using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation.PTC
{
    public class PtcFormatter
    {
        #region "Deserialize"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(Stream stream, Func<Stream, TValue> valueFunc) where TKey : unmanaged
        {
            Dictionary<TKey, TValue> dictionary = new();

            int count = DeserializeStructure<int>(stream);

            for (int i = 0; i < count; i++)
            {
                TKey key = DeserializeStructure<TKey>(stream);
                TValue value = valueFunc(stream);

                dictionary.Add(key, value);
            }

            return dictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> DeserializeList<T>(Stream stream) where T : unmanaged
        {
            List<T> list = new();

            int count = DeserializeStructure<int>(stream);

            for (int i = 0; i < count; i++)
            {
                T item = DeserializeStructure<T>(stream);

                list.Add(item);
            }

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeStructure<T>(Stream stream) where T : unmanaged
        {
            T structure = default(T);

            Span<T> spanT = MemoryMarshal.CreateSpan(ref structure, 1);
            stream.Read(MemoryMarshal.AsBytes(spanT));

            return structure;
        }
        #endregion

        #region "GetSerializeSize"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSerializeSizeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, Func<TValue, int> valueFunc) where TKey : unmanaged
        {
            int size = 0;

            size += Unsafe.SizeOf<int>();

            foreach ((_, TValue value) in dictionary)
            {
                size += Unsafe.SizeOf<TKey>();
                size += valueFunc(value);
            }

            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSerializeSizeList<T>(List<T> list) where T : unmanaged
        {
            int size = 0;

            size += Unsafe.SizeOf<int>();

            size += list.Count * Unsafe.SizeOf<T>();

            return size;
        }
        #endregion

        #region "Serialize"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeDictionary<TKey, TValue>(Stream stream, Dictionary<TKey, TValue> dictionary, Action<Stream, TValue> valueAction) where TKey : unmanaged
        {
            SerializeStructure<int>(stream, dictionary.Count);

            foreach ((TKey key, TValue value) in dictionary)
            {
                SerializeStructure<TKey>(stream, key);
                valueAction(stream, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeList<T>(Stream stream, List<T> list) where T : unmanaged
        {
            SerializeStructure<int>(stream, list.Count);

            foreach (T item in list)
            {
                SerializeStructure<T>(stream, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeStructure<T>(Stream stream, T structure) where T : unmanaged
        {
            Span<T> spanT = MemoryMarshal.CreateSpan(ref structure, 1);
            stream.Write(MemoryMarshal.AsBytes(spanT));
        }
        #endregion
    }
}