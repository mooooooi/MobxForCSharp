using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mobx.Core
{
    public interface IObserver
    {
        int Length { get; }
        bool this[int index] { get; set; }
        void Clear();
    }

    public enum ObserverMode
    {
        Set = 1 << 0, Get = 1 << 1, GetSet = Set | Get
    }

    public unsafe struct Observer64
    {
        const int k_MaskLength = 2;
        const int k_MaskPerLength = sizeof(int);
        fixed int mask[k_MaskLength];
        public readonly int Length => k_MaskLength * k_MaskPerLength;
        public ObserverMode Mode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Mark(int index)
        {
            mask[index / k_MaskPerLength] |= 1 << (index % k_MaskPerLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(int index, ref T field, in T value)
        {
            if ((Mode & ObserverMode.Set) > 0) Mark(index);
            field = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index, ref T field)
        {
            if ((Mode & ObserverMode.Get) > 0) Mark(index);
            return ref field;
        }

        public void Clear()
        {
            mask[0] = 0;
            mask[1] = 0;
        }
    }

    public unsafe partial interface IData
    {
        int a { get; set; }
        string b { get; set; }
    }

    public partial interface IData
    {
        public struct Implement : IData
        {
            private Observer64 __o__;

            private int a__k__BackingField;
            public int a
            {
                get => __o__.Get(0, ref a__k__BackingField);
                set => __o__.Set(0, ref a__k__BackingField, value);
            }

            private string b__k__BackingField;
            public string b
            {
                get => __o__.Get(1, ref b__k__BackingField);
                set => __o__.Set(1, ref b__k__BackingField, value);
            }
        }

        public static Implement Create() => new Implement();
    }
}
