using System.Collections.Generic;
using UnityEngine;

namespace Alchemy.UI
{
    public class AL_UIPool:MonoBehaviour
    {
        [SerializeField]  private GameObject _uiPrefab;
        private Queue<AL_IPoolObject> _pool = new Queue<AL_IPoolObject>();

        public T GetOne<T>()
        {
            AL_IPoolObject obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateObj();
            }
            return (T) obj;
        }

        public void RePool(AL_IPoolObject go)
        {
            if (go == null) return;
            go.RePool(this.transform);
            _pool.Enqueue(go);
        }

        protected AL_IPoolObject CreateObj()
        {
            var obj = GameObject.Instantiate(_uiPrefab, this.transform);
            if (obj != null)
            {
                return obj.GetComponent<AL_IPoolObject>();
            }
            return null;
        }
    }

    public interface AL_IPoolObject
    {
        public void RePool(Transform parent);
    }
}