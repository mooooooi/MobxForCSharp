using System;
using System.Collections.Specialized;

namespace Higo.Mobx
{
    public struct ActionScope : IDisposable
    {
        private Store m_store;
        private BitVector32 m_previousSetterFlag;
        public ActionScope(Store store)
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
                if ((k.Data & flag.Data) != k.Data) continue;
                foreach (var info in v)
                {
                    var fail = false;
                    foreach (var c in info.Condition)
                    {
                        if ((m_store.m_setterDeps[c.index].Data & c.deps.Data) != c.deps.Data)
                        {
                            fail = true;
                            break;
                        }
                    }
                    if (!fail)
                    {
                        info.Action();
                    }
                }
            }

            m_store.m_setterFlag = m_previousSetterFlag;
        }
    }

}