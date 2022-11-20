using System;

namespace Higo.Mobx
{
    public interface IStore
    {
        ref T GetValue<T>(ref ObservableValue<T> observable, ref T field);
        void SetValue<T>(ref ObservableValue<T> observable, ref T field, in T newValue);
        void AutoRun(Action onReaction);
        IDisposable CreateActionScope();
        void Bind<T>(in T observable) where T : IObservableForStore;
        int GetObjectId();
    }
}