using System;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public interface IStore
    {
        ref T GetValue<T>(in ParentInfo info, ref T field);
        void SetValue<T>(in ParentInfo info, ref T field, in T newValue);
        void AutoRun(Action onReaction);
        IDisposable CreateActionScope();
        void Bind<T>(in T observable) where T : IObservableForStore;
        int GetObjectId();
        void CombineSetterFlag(int objectId, int flag);
        void CombineGetterFlag(int objectId, int flag);
    }
}