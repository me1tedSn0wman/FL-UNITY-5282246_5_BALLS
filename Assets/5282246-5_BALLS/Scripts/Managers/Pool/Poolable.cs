using UnityEngine;

namespace Utils.Pool
{
    public class Poolable : MonoBehaviour
    {
        public int initialPoolCapacity = 10;
        public Pool<Poolable> pool;

        protected virtual void Repool()
        {
            transform.SetParent(PoolManager.Instance.transform, false);
            pool.Return(this);
        }

        public static void TryPool(GameObject gameObject)
        {
            Poolable poolable = gameObject.GetComponent<Poolable>();
            if (poolable != null
                && poolable.pool != null
                && PoolManager.instanceExists
                )
            {
                poolable.Repool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static T TryGetPoolable<T>(GameObject prefab) where T : Component
        {
            Poolable poolable = prefab.GetComponent<Poolable>();
            T instance = null;
//            Debug.Log(poolable + "___" + PoolManager.instanceExists);
            if (poolable != null &&
                PoolManager.instanceExists
                )
            {
//                Debug.Log("Get from Pool");
                instance = PoolManager.Instance.GetPoolable(poolable).GetComponent<T>();
            }
            else
            {
//                Debug.Log("create");
                instance = Instantiate(prefab).GetComponent<T>();
            }
            return instance;
        }

        public static GameObject TryGetPoolable(GameObject prefab)
        {
            Poolable poolable = prefab.GetComponent<Poolable>();

            GameObject instance = null;
            if (poolable != null && PoolManager.instanceExists)
            {
                instance = PoolManager.Instance.GetPoolable(poolable).gameObject;
            }
            else
            {
                instance = Instantiate(prefab);
            }
            return instance;
        }
    }
}