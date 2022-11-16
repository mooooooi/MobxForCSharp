using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Higo.Mobx
{
    public interface IReadonlyProxyValue<T>
    {
        public T Value { get; }
    }

    public struct ProxyValue<T> : IReadonlyProxyValue<T>
    {
        #region members
        private bool initialized;
        private int index;  
        private ObservableBase observable;
        internal T value;
        #endregion

        public T Value { get => doGet(); set => doSet(value); }
        public ProxyValue(ObservableBase observable, T defaultValue = default)
        {
            initialized = true;
            value = defaultValue;
            this.observable = observable;
            index = observable.count++;
        }

        private void doSet(T newVal)
        {
            Debug.Assert(initialized, "ProxtValue must be initlize!");
            observable.Set(index, ref value, newVal);
        }

        private T doGet()
        {
            Debug.Assert(initialized, "ProxtValue must be initlize!");
            return observable.Get(index, in value);
        }

        public IReadonlyProxyValue<T> Readonly() => this;
    }

    public abstract class ObservableBase
    {
        internal bool @readonly = true;
        internal int count;
        internal BitVector32 setDirtyFlag;
        internal BitVector32 getDirtyFlag;

        internal void Set<T>(int index, ref T member, in T newVal)
        {
            if (@readonly) throw new Exception($"Please modify value use CreateActionScope)!");
            setDirtyFlag[1 << index] = true;
            member = newVal;
        }

        internal T Get<T>(int index, in T member)
        {
            getDirtyFlag[1 << index] = true;
            return member;
        }

        protected ProxyValue<T> CreateValue<T>(T defaultValue = default)
        {
            return new ProxyValue<T>(this, defaultValue);
        }
    }

    public abstract class Observable<T> : ObservableBase
        where T : ObservableBase
    {
        public delegate void ReactionCallback(T observable);
        internal List<(int, ReactionCallback)> m_reactions = new List<(int, ReactionCallback)>();
        public void AutoRun(ReactionCallback action)
        {
            using (var scope = new GetterScope(this))
            {
                action(this as T);
                m_reactions.Add((scope.DirtyFlag.Data, action));
            }
        }

        public ActionScope<T> CreateActionScope() => new ActionScope<T>(this);
    }

    public struct GetterScope : IDisposable
    {
        ObservableBase m_obervable;
        BitVector32 m_previousFlag;
        public BitVector32 DirtyFlag => m_obervable.getDirtyFlag;
        public GetterScope(ObservableBase observable)
        {
            m_obervable = observable;
            m_previousFlag = observable.getDirtyFlag;
            observable.getDirtyFlag = new BitVector32();
        }

        public void Dispose()
        {
            m_obervable.getDirtyFlag = m_previousFlag;
        }
    }

    public struct ActionScope<T> : IDisposable
        where T : ObservableBase
    {
        Observable<T> m_obervable;
        BitVector32 m_previousFlag;
        bool m_previousReadonly;
        public BitVector32 DirtyFlag => m_obervable.setDirtyFlag;
        public ActionScope(Observable<T> observable)
        {
            m_obervable = observable;
            m_previousFlag = observable.setDirtyFlag;
            m_previousReadonly = observable.@readonly;
            observable.setDirtyFlag = new BitVector32();
            observable.@readonly = false;
        }

        public void Dispose()
        {
            var flag = m_obervable.setDirtyFlag;
            m_obervable.setDirtyFlag = m_previousFlag;
            m_obervable.@readonly = m_previousReadonly;
            foreach (var (maskNeeded, reaction) in m_obervable.m_reactions)
            {
                if (!flag[maskNeeded]) continue;
                reaction(m_obervable as T);
            }
        }
    }
}