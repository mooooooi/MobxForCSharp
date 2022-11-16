using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Higo.Mobx2
{
    public static class Store
    {
        public const string k_AssertObservableValueNotInitialized = "ObservableValue must be initialize!";
        internal static int count = 1;
        internal static void SetValue<T>(ref ObservableValue<T> observable, T newVal)
        {
            Debug.Assert(!observable.IsInitialized, k_AssertObservableValueNotInitialized);
            observable.value = newVal;
        }

        internal static T GetValue<T>(ref ObservableValue<T> observable)
        {
            Debug.Assert(!observable.IsInitialized, k_AssertObservableValueNotInitialized);
            return observable.value;
        }

        public static void Bind<TObservable>(IObservable observable) where TObservable : IObservable
        {
            observable.init(0, count++);
        }

        public static void AutoRun()
        {
            
        }
    }

    public interface IObservable
    {
        public bool IsInitialized { get; }
        internal void init(int parentIndex, int index);
    }

    public struct ObservableValue<T> : IObservable
    {
        internal int parentIndex;
        internal int index;
        internal T value;

        public bool IsInitialized => index == 0;
        public T Value
        {
            set => Store.SetValue(ref this, value);
            get => Store.GetValue(ref this);
        }

        void IObservable.init(int parentIndex, int index)
        {
            this.parentIndex = parentIndex;
            this.index = index;
        }
    }

    public abstract class ObservableObject : IObservable
    {
        internal int parentIndex;
        internal int index;

        public bool IsInitialized => index == 0;

        public void Bind<TObservable>(ref TObservable observable) where TObservable : IObservable
        {
            observable.init(index, Store.count++);
            OnBind();
        }

        protected abstract void OnBind();

        void IObservable.init(int parentIndex, int index)
        {
            this.parentIndex = parentIndex;
            this.index = index;
        }
    }
}