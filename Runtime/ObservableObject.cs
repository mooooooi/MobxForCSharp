using System;

namespace Higo.Mobx
{
    public class ObservableObject : IObservable, IObservableForStore
    {
        internal Store m_store;
        internal ParentInfo m_parentInfo;
        internal int m_objectId;
        internal int m_fieldCount;
        public bool IsInitialized => m_store != null;

        void IObservableForStore.init(Store store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
            m_objectId = store.getObjectId();
        }

        protected void Bind<T>(ref T observable) where T : IObservableForStore
        {
            if (m_fieldCount >= 32) throw new Exception("max field count is 32!");
            var parentInfo = new ParentInfo()
            {
                ObjectId = m_objectId,
                FieldId = m_fieldCount++,
            };
            observable.init(m_store, in parentInfo);
        }
    }
}