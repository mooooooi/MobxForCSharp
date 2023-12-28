using System;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public struct ActionScope32 : IDisposable
    {
        private Store32 m_store;
        private BitVector32 m_previousSetterFlag;
        public ActionScope32(Store32 store)
        {
            m_store = store;
            m_previousSetterFlag = store.m_setterFlag;
            m_previousSetterFlag = default;
        }

        public void Dispose()
        {
            var flag = m_store.m_setterFlag;
            foreach (var (k, v) in m_store.m_reactions)
            {
                if ((k.Data & flag.Data) != flag.Data) continue;
                foreach (var info in v)
                {
                    foreach (var c in info.Condition)
                    {
                        if ((c.deps.Data & m_store.m_setterDeps[c.index].Data) > 0)
                        {
                            info.Action();
                            break;
                        }

                    }
                }
            }

            m_store.m_setterFlag = m_previousSetterFlag;
        }
    }

}