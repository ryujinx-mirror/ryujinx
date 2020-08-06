using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    public struct Array1<T> : IArray<T> where T : unmanaged
    {
        T _e0;
        public int Length => 1;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 1);
    }
    public struct Array2<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array1<T> _other;
#pragma warning restore CS0169
        public int Length => 2;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 2);
    }
    public struct Array3<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array2<T> _other;
#pragma warning restore CS0169
        public int Length => 3;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 3);
    }
    public struct Array4<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array3<T> _other;
#pragma warning restore CS0169
        public int Length => 4;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 4);
    }
    public struct Array5<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array4<T> _other;
#pragma warning restore CS0169
        public int Length => 5;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 5);
    }
    public struct Array6<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array5<T> _other;
#pragma warning restore CS0169
        public int Length => 6;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 6);
    }
    public struct Array7<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array6<T> _other;
#pragma warning restore CS0169
        public int Length => 7;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 7);
    }
    public struct Array8<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array7<T> _other;
#pragma warning restore CS0169
        public int Length => 8;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 8);
    }
    public struct Array9<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array8<T> _other;
#pragma warning restore CS0169
        public int Length => 9;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 9);
    }
    public struct Array10<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array9<T> _other;
#pragma warning restore CS0169
        public int Length => 10;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 10);
    }
    public struct Array11<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array10<T> _other;
#pragma warning restore CS0169
        public int Length => 11;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 11);
    }
    public struct Array12<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array11<T> _other;
#pragma warning restore CS0169
        public int Length => 12;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 12);
    }
    public struct Array13<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array12<T> _other;
#pragma warning restore CS0169
        public int Length => 13;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 13);
    }
    public struct Array14<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array13<T> _other;
#pragma warning restore CS0169
        public int Length => 14;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 14);
    }
    public struct Array15<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array14<T> _other;
#pragma warning restore CS0169
        public int Length => 15;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 15);
    }
    public struct Array16<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array15<T> _other;
#pragma warning restore CS0169
        public int Length => 16;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 16);
    }
    public struct Array17<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array16<T> _other;
#pragma warning restore CS0169
        public int Length => 17;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 17);
    }
    public struct Array18<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array17<T> _other;
#pragma warning restore CS0169
        public int Length => 18;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 18);
    }
    public struct Array19<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array18<T> _other;
#pragma warning restore CS0169
        public int Length => 19;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 19);
    }
    public struct Array20<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array19<T> _other;
#pragma warning restore CS0169
        public int Length => 20;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 20);
    }
    public struct Array21<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array20<T> _other;
#pragma warning restore CS0169
        public int Length => 21;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 21);
    }
    public struct Array22<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array21<T> _other;
#pragma warning restore CS0169
        public int Length => 22;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 22);
    }
    public struct Array23<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array22<T> _other;
#pragma warning restore CS0169
        public int Length => 23;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 23);
    }
    public struct Array24<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array23<T> _other;
#pragma warning restore CS0169
        public int Length => 24;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 24);
    }
    public struct Array25<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array24<T> _other;
#pragma warning restore CS0169
        public int Length => 25;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 25);
    }
    public struct Array26<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array25<T> _other;
#pragma warning restore CS0169
        public int Length => 26;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 26);
    }
    public struct Array27<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array26<T> _other;
#pragma warning restore CS0169
        public int Length => 27;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 27);
    }
    public struct Array28<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array27<T> _other;
#pragma warning restore CS0169
        public int Length => 28;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 28);
    }
    public struct Array29<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array28<T> _other;
#pragma warning restore CS0169
        public int Length => 29;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 29);
    }
    public struct Array30<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array29<T> _other;
#pragma warning restore CS0169
        public int Length => 30;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 30);
    }
    public struct Array31<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array30<T> _other;
#pragma warning restore CS0169
        public int Length => 31;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 31);
    }
    public struct Array32<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array31<T> _other;
#pragma warning restore CS0169
        public int Length => 32;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 32);
    }
    public struct Array33<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array32<T> _other;
#pragma warning restore CS0169
        public int Length => 33;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 33);
    }
    public struct Array34<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array33<T> _other;
#pragma warning restore CS0169
        public int Length => 34;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 34);
    }
    public struct Array35<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array34<T> _other;
#pragma warning restore CS0169
        public int Length => 35;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 35);
    }
    public struct Array36<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array35<T> _other;
#pragma warning restore CS0169
        public int Length => 36;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 36);
    }
    public struct Array37<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array36<T> _other;
#pragma warning restore CS0169
        public int Length => 37;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 37);
    }
    public struct Array38<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array37<T> _other;
#pragma warning restore CS0169
        public int Length => 38;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 38);
    }
    public struct Array39<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array38<T> _other;
#pragma warning restore CS0169
        public int Length => 39;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 39);
    }
    public struct Array40<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array39<T> _other;
#pragma warning restore CS0169
        public int Length => 40;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 40);
    }
    public struct Array41<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array40<T> _other;
#pragma warning restore CS0169
        public int Length => 41;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 41);
    }
    public struct Array42<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array41<T> _other;
#pragma warning restore CS0169
        public int Length => 42;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 42);
    }
    public struct Array43<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array42<T> _other;
#pragma warning restore CS0169
        public int Length => 43;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 43);
    }
    public struct Array44<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array43<T> _other;
#pragma warning restore CS0169
        public int Length => 44;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 44);
    }
    public struct Array45<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array44<T> _other;
#pragma warning restore CS0169
        public int Length => 45;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 45);
    }
    public struct Array46<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array45<T> _other;
#pragma warning restore CS0169
        public int Length => 46;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 46);
    }
    public struct Array47<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array46<T> _other;
#pragma warning restore CS0169
        public int Length => 47;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 47);
    }
    public struct Array48<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array47<T> _other;
#pragma warning restore CS0169
        public int Length => 48;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 48);
    }
    public struct Array49<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array48<T> _other;
#pragma warning restore CS0169
        public int Length => 49;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 49);
    }
    public struct Array50<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array49<T> _other;
#pragma warning restore CS0169
        public int Length => 50;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 50);
    }
    public struct Array51<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array50<T> _other;
#pragma warning restore CS0169
        public int Length => 51;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 51);
    }
    public struct Array52<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array51<T> _other;
#pragma warning restore CS0169
        public int Length => 52;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 52);
    }
    public struct Array53<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array52<T> _other;
#pragma warning restore CS0169
        public int Length => 53;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 53);
    }
    public struct Array54<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array53<T> _other;
#pragma warning restore CS0169
        public int Length => 54;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 54);
    }
    public struct Array55<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array54<T> _other;
#pragma warning restore CS0169
        public int Length => 55;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 55);
    }
    public struct Array56<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array55<T> _other;
#pragma warning restore CS0169
        public int Length => 56;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 56);
    }
    public struct Array57<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array56<T> _other;
#pragma warning restore CS0169
        public int Length => 57;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 57);
    }
    public struct Array58<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array57<T> _other;
#pragma warning restore CS0169
        public int Length => 58;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 58);
    }
    public struct Array59<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array58<T> _other;
#pragma warning restore CS0169
        public int Length => 59;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 59);
    }
    public struct Array60<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array59<T> _other;
#pragma warning restore CS0169
        public int Length => 60;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 60);
    }
    public struct Array61<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array60<T> _other;
#pragma warning restore CS0169
        public int Length => 61;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 61);
    }
    public struct Array62<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array61<T> _other;
#pragma warning restore CS0169
        public int Length => 62;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 62);
    }
    public struct Array63<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array62<T> _other;
#pragma warning restore CS0169
        public int Length => 63;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 63);
    }
    public struct Array64<T> : IArray<T> where T : unmanaged
    {
#pragma warning disable CS0169
        T _e0;
        Array63<T> _other;
#pragma warning restore CS0169
        public int Length => 64;
        public ref T this[int index] => ref ToSpan()[index];
        public Span<T> ToSpan() => MemoryMarshal.CreateSpan(ref _e0, 64);
    }

}
