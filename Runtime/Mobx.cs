using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public struct ParentInfo
    {
        public int ObjectId;
        public int FieldId;
    }

    public class ReactionInfo
    {
        public List<(int index, BitVector32 deps)> Condition;
        public Action Action;
    }

    public interface IObservableForStore
    {
        internal void init(IStore store, in ParentInfo parentInfo);
    }

    public interface IObservable
    {
        public bool IsInitialized { get; }
        public IStore Store { get; }
    }
}