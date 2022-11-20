using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace Higo.Mobx
{
    public class ObservableList<T> : IObservable, IObservableForStore, IReadOnlyList<T>, IList<T>
    {
        internal IStore m_store;
        protected ParentInfo m_parentInfo;
        protected int m_objectId;
        protected T[] m_items;
        protected int m_count;

        public bool IsInitialized => m_store != null;
        public int Count => m_count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => m_store.GetValue(new ParentInfo() { ObjectId = m_objectId, FieldId = index }, ref m_items[index]);
            set => m_store.SetValue(new ParentInfo() { ObjectId = m_objectId, FieldId = index }, ref m_items[index], in value);
        }

        void IObservableForStore.init(IStore store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
            m_objectId = store.GetObjectId();
            m_items = new T[32];
        }

        public IDisposable CreateActionScope() => m_store.CreateActionScope();

        public IEnumerator<T> GetEnumerator() => new ArrayIEnumrator(this);

        IEnumerator IEnumerable.GetEnumerator() => new ArrayIEnumrator(this);

        public int IndexOf(T item)
        {
            if (m_count == 0) return -1;
            for (var i = 0; i < m_count; i++)
            {
                if (Comparer<T>.Default.Compare(item, m_items[i]) == 0)
                    return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            if (m_count >= m_items.Length)
                throw new IndexOutOfRangeException();
            var index = m_count++;
            m_store.SetValue(new ParentInfo() { ObjectId = m_objectId, FieldId = index }, ref m_items[index], in item);
        }

        public void Clear()
        {
            m_count = 0;
            Array.Clear(m_items, 0, m_count);
            m_store.CombineSetterFlag(m_objectId, (1 << m_count) - 1);
        }

        public bool Contains(T item)
        {
            if (m_count == 0) return false;
            for (var i = 0; i < m_count; i++)
            {
                if (Comparer<T>.Default.Compare(item, m_items[i]) == 0)
                    return true;
            }
            return false;
        }

        public bool TryGetValue(int index, out T value)
        {
            if (index < m_items.Length)
            {
                m_store.CombineGetterFlag(m_objectId, 1 << index);
            }
            var isInRange = index < m_count;
            value = isInRange ? m_items[index] : default;

            return isInRange;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (m_count - arrayIndex + 1 > array.Length) throw new IndexOutOfRangeException();
            for (var i = arrayIndex; i < m_count; i++)
            {
                array[i - arrayIndex] = m_items[i];
            }
        }

        public struct ArrayIEnumrator : IEnumerator<T>
        {
            private ObservableList<T> m_observable;
            private int m_cursor;
            public T Current => m_observable[m_cursor];
            object IEnumerator.Current => m_observable[m_cursor];

            public ArrayIEnumrator(ObservableList<T> observable)
            {
                m_observable = observable;
                m_cursor = -1;
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                return ++m_cursor < m_observable.m_count;
            }

            public void Reset()
            {
                m_cursor = -1;
            }
        }
    }
}
