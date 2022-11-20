using System;

namespace Higo.Mobx
{
    public abstract class ObservableObject : IObservable, IObservableForStore
    {
        internal IStore m_store;
        internal ParentInfo m_parentInfo;
        internal int m_objectId;
        internal int m_fieldCount;
        public bool IsInitialized => m_store != null;

        protected abstract void OnBind();
        void IObservableForStore.init(IStore store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
            m_objectId = store.GetObjectId();
            OnBind();
        }

        protected void BindValue<T>(ref T observable) where T : struct, IObservableForStore
        {
            if (m_fieldCount >= 32) throw new Exception("max field count is 32!");
            var parentInfo = new ParentInfo()
            {
                ObjectId = m_objectId,
                FieldId = m_fieldCount++,
            };
            observable.init(m_store, in parentInfo);
        }

        protected void BindObject<T>(ref T observable) where T : IObservableForStore, new()
        {
            if (m_fieldCount >= 32) throw new Exception("max field count is 32!");
            if (observable == null)
                observable = new T();
            var parentInfo = new ParentInfo()
            {
                ObjectId = m_objectId,
                FieldId = m_fieldCount++,
            };
            observable.init(m_store, in parentInfo);
        }

        public IDisposable CreateActionScope() => m_store.CreateActionScope();
    }

    public static class ObservableObjectExt
    {
        public static void AutoRun<TObservable>(this TObservable observable, Action<TObservable> reaction)
            where TObservable : ObservableObject
        {
            observable.m_store.AutoRun(() => reaction(observable));
        }

        public static void AutoRun(this ObservableObject observable, Action reaction)
        {
            observable.m_store.AutoRun(reaction);
        }

        public static void AutoRun<T, TObservable>(this TObservable observable, Action<TObservable> reaction)
            where TObservable : ObservableList<T>
        {
            observable.m_store.AutoRun(() => reaction(observable));
        }

        public static void AutoRun<T>(this ObservableList<T> observable, Action reaction)
        {
            observable.m_store.AutoRun(reaction);
        }
    }
}