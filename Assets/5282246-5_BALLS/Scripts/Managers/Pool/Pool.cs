using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.Pool
{
    public class Pool<T>
    {
        protected Func<T> m_Factory;
        protected readonly Action<T> m_Reset;

        protected readonly List<T> m_Available;

        protected readonly List<T> m_All;

        public Pool(Func<T> factory, Action<T> reset, int initialCapacity)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            m_Available = new List<T>();
            m_All = new List<T>();
            m_Factory = factory;
            m_Reset = reset;

            if (initialCapacity > 0)
            {
                Grow(initialCapacity);
            }
        }

        public Pool(Func<T> factory)
            : this(factory, null, 0)
        {
        }

        public Pool(Func<T> factory, int initialCapacity)
            : this(factory, null, initialCapacity)
        {
        }

        public virtual T Get()
        {
            return Get(m_Reset);
        }

        public virtual T Get(Action<T> resetOverride)
        {
            if (m_Available.Count == 0)
            {
                Grow(1);
            }
            if (m_Available.Count == 0)
            {
                throw new InvalidOperationException("Failed to grow pool");
            }

            int itemIndex = m_Available.Count - 1;
            T item = m_Available[itemIndex];
            m_Available.RemoveAt(itemIndex);

            if (resetOverride != null)
            {
                resetOverride(item);
            }

            return item;
        }

        public virtual bool Contains(T pooledItem)
        {
            return m_All.Contains(pooledItem);
        }

        public virtual void Return(T pooledItem)
        {
            if (m_All.Contains(pooledItem)
                && !m_Available.Contains(pooledItem))
            {
                ReturnToPoolInternal(pooledItem);
            }
            else
            {
                throw new InvalidOperationException(
                    "Trying to return an item to a pool that does not contain it: "
                    + pooledItem + ", " + this);
            }
        }

        public virtual void ReturnAll()
        {
            ReturnAll(null);
        }

        public virtual void ReturnAll(Action<T> preReturn)
        {
            for (int i = 0; i < m_All.Count; i++)
            {
                T item = m_All[i];
                if (!m_Available.Contains(item))
                {
                    if (preReturn != null)
                    {
                        preReturn(item);
                    }
                    ReturnToPoolInternal(item);
                }
            }
        }

        public void Grow(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                AddNewElement();
            }
        }

        protected virtual void ReturnToPoolInternal(T element)
        {
            m_Available.Add(element);
        }

        protected virtual T AddNewElement()
        {
            T newElement = m_Factory();
            m_All.Add(newElement);
            m_Available.Add(newElement);

            return newElement;
        }

        protected static T DummyFactory()
        {
            return default(T);
        }

    }
    public class UnityComponentPool<T> : Pool<T>
        where T : Component
    {
        public UnityComponentPool(Func<T> factory, Action<T> reset, int initialCapacity)
            : base(factory, reset, initialCapacity)
        {
        }

        public UnityComponentPool(Func<T> factory)
            : base(factory)
        {
        }

        public UnityComponentPool(Func<T> factory, int initialCapacity)
            : base(factory, initialCapacity)
        {
        }

        public override T Get(Action<T> resetOverride)
        {
            T element = base.Get(resetOverride);

            element.gameObject.SetActive(true);

            return element;
        }

        protected override void ReturnToPoolInternal(T element)
        {
            element.gameObject.SetActive(false);

            base.ReturnToPoolInternal(element);
        }

        protected override T AddNewElement()
        {
            T newElement = base.AddNewElement();

            newElement.gameObject.SetActive(false);

            return newElement;
        }
    }
    
    public class GameObjectPool : Pool<GameObject>
    {

        public GameObjectPool(Func<GameObject> factory, Action<GameObject> reset, int initialCapacity)
            : base(factory, reset, initialCapacity)
        {
        }

        public GameObjectPool(Func<GameObject> factory)
            : base(factory)
        {
        }

        public GameObjectPool(Func<GameObject> factory, int initialCapacity)
            : base(factory, initialCapacity)
        {
        }

        public override GameObject Get(Action<GameObject> resetOverride)
        {
            GameObject element = base.Get(resetOverride);

            element.SetActive(true);

            return element;
        }

        protected override void ReturnToPoolInternal(GameObject element)
        {
            element.SetActive(false);

            base.ReturnToPoolInternal(element);
        }

        protected override GameObject AddNewElement()
        {
            GameObject newElement = base.AddNewElement();

            newElement.SetActive(false);

            return newElement;
        }
    }
    
    public class AutoGameObjectPrefabPool : GameObjectPool
    {
        protected readonly GameObject m_Prefab;
        protected readonly Action<GameObject> m_Initialize;
        
        GameObject PrefabFactory()
        {
            GameObject newElement = Object.Instantiate(m_Prefab);
            if (m_Initialize != null)
            {
                m_Initialize(newElement);
            }
        
            return newElement;
        }
        
        public AutoGameObjectPrefabPool(GameObject prefab)
            : this(prefab, null, null, 0)
        {
        }
        
        public AutoGameObjectPrefabPool(GameObject prefab, Action<GameObject> initialize)
            : this(prefab, initialize, null, 0)
        {
        }
        
        public AutoGameObjectPrefabPool(GameObject prefab, Action<GameObject> initialize, Action<GameObject> reset)
            : this(prefab, initialize, reset, 0)
        {
        }
        
        public AutoGameObjectPrefabPool(GameObject prefab, int initialCapacity)
            : this(prefab, null, null, initialCapacity)
        {
        }
        
        public AutoGameObjectPrefabPool(GameObject prefab, Action<GameObject> initialize, Action<GameObject> reset, int initialCapacity)
            : base(DummyFactory, reset, 0)
        {
            m_Initialize = initialize;
            m_Prefab = prefab;
            m_Factory = PrefabFactory;
            if (initialCapacity > 0)
            {
                Grow(initialCapacity);
            }
        }
    }
    
    public class AutoComponentPrefabPool<T> : UnityComponentPool<T>
        where T : Component
    {
        protected readonly T m_Prefab;
        protected readonly Action<T> m_Initialize;

        T PrefabFactory()
        {
            T newElement = Object.Instantiate(m_Prefab);
            if (m_Initialize != null)
            {
                m_Initialize(newElement);
            }

            return newElement;
        }

        public AutoComponentPrefabPool(T prefab)
            : this(prefab, null, null, 0)
        {
        }

        public AutoComponentPrefabPool(T prefab, Action<T> initialize)
            : this(prefab, initialize, null, 0)
        {
        }

        public AutoComponentPrefabPool(T prefab, Action<T> initialize, Action<T> reset)
            : this(prefab, initialize, reset, 0)
        {
        }

        public AutoComponentPrefabPool(T prefab, int initialCapacity)
            : this(prefab, null, null, initialCapacity)
        {
        }

        public AutoComponentPrefabPool(T prefab, Action<T> initialize, Action<T> reset, int initialCapacity)
            : base(DummyFactory, reset, 0)
        {
            m_Initialize = initialize;
            m_Prefab = prefab;
            m_Factory = PrefabFactory;
            if (initialCapacity > 0)
            {
                Grow(initialCapacity);
            }
        }
    }
}