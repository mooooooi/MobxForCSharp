namespace Higo.Mobx
{
    public struct ObservableValue<T> : IObservable, IObservableForStore
    {
        internal IStore m_store;
        internal ParentInfo m_parentInfo;
        internal T m_value;
        public T Value
        {
            get => m_store.GetValue(in m_parentInfo, ref m_value);
            set => m_store.SetValue(in m_parentInfo, ref m_value, in value);
        }

        public bool IsInitialized => m_store != null;

        public IStore Store => m_store;

        void IObservableForStore.init(IStore store, in ParentInfo parentInfo)
        {
            m_store = store;
            m_parentInfo = parentInfo;
        }
    }
}