namespace Higo.Mobx
{
    public struct ObservableValue<T> : IObservable, IObservableForStore
    {
        internal Store m_store;
        internal ParentInfo m_parentInfo;
        internal T m_value;
        public T Value
        {
            get => m_store.GetValue(ref m_value, in m_parentInfo);
            set => m_store.SetValue(ref m_value, in value, in m_parentInfo);
        }

        public bool IsInitialized => m_store != null;

        void IObservableForStore.init(Store store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
        }
    }
}