using Codice.CM.Common.Merge;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public class Store32 : IStore
    {
        internal List<BitVector32> m_getterDeps = new();
        internal List<BitVector32> m_setterDeps = new();
        internal BitVector32 m_getterFlag;
        internal BitVector32 m_setterFlag;

        internal Dictionary<BitVector32, List<ReactionInfo>> m_reactions = new();
        internal int m_fieldCount;

        public int MaxFieldCount => 32;

        public ref T GetValue<T>(in ParentInfo parentInfo, ref T field)
        {
            var dep = m_getterFlag[1 << parentInfo.ObjectId]
                ? m_getterDeps[parentInfo.ObjectId] : default;
            m_getterFlag[1 << parentInfo.ObjectId] = true;
            dep[1 << parentInfo.FieldId] = true;
            m_getterDeps[parentInfo.ObjectId] = dep;

            return ref field;
        }

        public void SetValue<T>(in ParentInfo parentInfo, ref T field, in T newValue)
        {
            var dep = m_setterFlag[1 << parentInfo.ObjectId]
                ? m_setterDeps[parentInfo.ObjectId]
                : default;
            m_setterFlag[1 << parentInfo.ObjectId] = true;
            dep[1 << parentInfo.FieldId] = true;
            m_setterDeps[parentInfo.ObjectId] = dep;

            field = newValue;
        }

        public void CombineSetterFlag(int objectId, int flag)
        {
            var dep = m_setterFlag[1 << objectId]
                ? m_setterDeps[objectId] : default;
            m_setterDeps[objectId] = new BitVector32(dep.Data | flag);
            m_setterFlag[1 << objectId] = true;
        }

        public void CombineGetterFlag(int objectId, int flag)
        {
            var dep = m_getterFlag[1 << objectId]
                ? m_getterDeps[objectId] : default;
            m_getterDeps[objectId] = new BitVector32(dep.Data | flag);
            m_getterFlag[1 << objectId] = true;
        }

        public void AutoRun(Action onReaction)
        {
            var previousGetterFlag = m_getterFlag;
            m_getterFlag = default;
            onReaction();

            var flagData = m_getterFlag.Data;
            var reactionInfo = new ReactionInfo();

            reactionInfo.Condition = new List<(int index, BitVector32 deps)>();
            reactionInfo.Action = onReaction;

            int num = 0;
            while (flagData > 0)
            {
                if ((flagData & 0b1) > 0 && m_getterDeps[num].Data > 0)
                    reactionInfo.Condition.Add((num, m_getterDeps[num]));
                flagData >>= 1;
                num++;
            }

            if (!m_reactions.TryGetValue(m_getterFlag, out var list))
                m_reactions[m_getterFlag] = list = new List<ReactionInfo>();
            list.Add(reactionInfo);

            m_getterFlag = previousGetterFlag;
        }

        public IDisposable CreateActionScope() => new ActionScope32(this);

        public void Bind<T>(in T observable) where T : IObservableForStore
        {
            if (m_fieldCount >= MaxFieldCount) throw new Exception($"max field count is {MaxFieldCount}!");
            var parentInfo = new ParentInfo()
            {
                ObjectId = 0,
                FieldId = m_fieldCount++,
            };
            observable.init(this, in parentInfo);
        }

        public int GetObjectId()
        {
            var ret = m_getterDeps.Count;
            m_getterDeps.Add(default);
            m_setterDeps.Add(default);
            return ret;
        }

        public static TObservable AsRoot<TObservable>()
            where TObservable : IObservableForStore, new()
        {
            var store = new Store32();
            var observable = new TObservable();
            store.Bind(observable);
            return observable;
        }
    }
}