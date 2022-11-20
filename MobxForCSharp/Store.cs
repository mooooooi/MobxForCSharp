using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public class Store
    {
        internal List<BitVector32> m_getterDeps = new List<BitVector32>();
        internal List<BitVector32> m_setterDeps = new List<BitVector32>();
        internal BitVector32 m_getterFlag;
        internal BitVector32 m_setterFlag;

        internal Dictionary<BitVector32, List<ReactionInfo>> m_reactions = new Dictionary<BitVector32, List<ReactionInfo>>();
        internal int m_fieldCount;

        public ref T GetValue<T>(ref T field, in ParentInfo parentInfo)
        {
            var dep = m_getterFlag[1 << parentInfo.ObjectId]
                ? m_getterDeps[parentInfo.ObjectId] : default;
            m_getterFlag[1 << parentInfo.ObjectId] = true;
            dep[1 << parentInfo.FieldId] = true;
            m_getterDeps[parentInfo.ObjectId] = dep;

            return ref field;
        }

        public void SetValue<T>(ref T field, in T newValue, in ParentInfo parentInfo)
        {
            var dep = m_setterFlag[1 << parentInfo.ObjectId]
                ? m_setterDeps[parentInfo.ObjectId]
                : default;
            m_setterFlag[1 << parentInfo.ObjectId] = true;
            dep[1 << parentInfo.FieldId] = true;
            m_setterDeps[parentInfo.ObjectId] = dep;

            field = newValue;
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
                if ((flagData & 0b1) > 0)
                    reactionInfo.Condition.Add((num, m_getterDeps[num]));
                flagData >>= 1;
                num++;
            }

            if (!m_reactions.TryGetValue(m_getterFlag, out var list))
                m_reactions[m_getterFlag] = list = new List<ReactionInfo>();
            list.Add(reactionInfo);

            m_getterFlag = previousGetterFlag; 
        }

        public ActionScope CreateActionScope() => new ActionScope(this);

        public void Bind<T>(in T observable) where T : IObservableForStore
        {
            if (m_fieldCount >= 32) throw new Exception("max field count is 32!");
            var parentInfo = new ParentInfo()
            {
                ObjectId = 0,
                FieldId = m_fieldCount++,
            };
            observable.init(this, in parentInfo);
        }

        internal int getObjectId()
        {
            var ret = m_getterDeps.Count;
            m_getterDeps.Add(default);
            m_setterDeps.Add(default);
            return ret;
        }
    }
}