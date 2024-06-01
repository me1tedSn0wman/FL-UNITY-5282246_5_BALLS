using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utils.Pool
{
    public class PoolManager : Singleton<PoolManager>
    {
        public List<Poolable> poolables;

        protected Dictionary<Poolable, AutoComponentPrefabPool<Poolable>> m_Pools;

        public Poolable GetPoolable(Poolable poolablePrefab)
        {
            if (!m_Pools.ContainsKey(poolablePrefab))
            {
                m_Pools.Add(poolablePrefab, new AutoComponentPrefabPool<Poolable>(
                    poolablePrefab, Initialize, null, poolablePrefab.initialPoolCapacity
                    ));
            }

            AutoComponentPrefabPool<Poolable> pool = m_Pools[poolablePrefab];
            Poolable spawnedInstance = pool.Get();

            spawnedInstance.pool = pool;
            return spawnedInstance;
        }

        public void ReturnPoolable(Poolable poolable)
        {
            poolable.pool.Return(poolable);
        }

        protected void Start()
        {
            m_Pools = new Dictionary<Poolable, AutoComponentPrefabPool<Poolable>>();

            foreach (Poolable poolable in poolables)
            {
                if (poolable == null)
                {
                    continue;
                }
                m_Pools.Add(
                    poolable,
                    new AutoComponentPrefabPool<Poolable>(
                        poolable,
                        Initialize,
                        null,
                        poolable.initialPoolCapacity
                        )
                    );
            }
        }

        void Initialize(Component poolable)
        {
            poolable.transform.SetParent(transform, false);
        }
    }
}