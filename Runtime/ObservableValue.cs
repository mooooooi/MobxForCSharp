namespace Higo.Mobx
{
    public struct ObservableValue<T> : IObservable, IObservableForStore
    {
        internal Store m_store;
        internal ParentInfo m_parentInfo;
        internal T m_value;
        public T Value
        {
            get => m_store.GetValue(ref this, ref m_value);
            set => m_store.SetValue(ref this, ref m_value, in value);
        }

        public bool IsInitialized => m_store != null;

        void IObservableForStore.init(Store store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
        }
    }
}