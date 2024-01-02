using System;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mobx.Core
{
    public static class M
    {
        private static uint m_GlobalBatchDepth = 0;
        private static uint m_IdGenerator = 0;

        public ref struct BatchDepthScope
        {
            private bool m_Created;

            public uint BatchDepth => m_GlobalBatchDepth;

            public static BatchDepthScope Create()
            {
                m_GlobalBatchDepth++;
                return new BatchDepthScope() { m_Created = true };
            }

            public void Dispose()
            {
                if (!m_Created) return;
                m_GlobalBatchDepth--;
                m_Created = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartBatch()
        {
            m_GlobalBatchDepth++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndBatch()
        {
            m_GlobalBatchDepth--;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(ref T field, in T value)
        {
            field = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetValue<T>(ref T field)
        {
            StartBatch();
            EndBatch();
            return ref field;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInBatching() => m_GlobalBatchDepth != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReportObervered(ref uint m_Id)
        {
            if (!IsInBatching()) return;
            if (m_Id == 0) m_Id = ++m_IdGenerator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReportChanged(ref uint m_Id)
        {
            if (!IsInBatching()) return;
            if (m_Id == 0) m_Id = ++m_IdGenerator;
        }
    }

    public class ObservableValue<T>
    {
        private uint m_Id;
        [AllowNull]
        private T m_Field;

        public ObservableValue()
        {
            m_Id = 0;
            m_Field = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(in T value)
        {
            M.ReportChanged(ref m_Id);
            m_Field = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue()
        {
            M.ReportObervered(ref m_Id);
            return m_Field;
        }
    }

    public struct TestData
    {
        private ObservableValue<int> __a;
        public int a { get => __a.GetValue(); set => __a.SetValue(value); }
    }
}
