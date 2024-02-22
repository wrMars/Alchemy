using System;
using System.Collections.Generic;
using Alchemy.ExtraClass;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 只是为了处理unity 编辑器拓展开发时编译过后非序列化数据会丢失的问题；正常情况下不要用这个类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    [Serializable]
    public class AL_SerializableDictionary<TKey, TVal>
    {
        [SerializeField]
        private List<TKey> _keyList = new List<TKey>();
        [SerializeField]
        private List<TVal> _valList = new List<TVal>();

        private Dictionary<TKey, TVal> _dicForCount = new Dictionary<TKey, TVal>();

        public Dictionary<TKey, TVal> Data => CheckFitDic();
        public int Count => _keyList.Count;
        
        public AL_SerializableDictionary()
        {
            CheckFitDic();
        }
        
        /// <summary>
        /// 每个使用_dicForCount的方法执行前去判断下，避免编辑器拓展开发时候，编译过后非序列化数据会丢失的问题
        /// </summary>
        private Dictionary<TKey, TVal> CheckFitDic()
        {
#if UNITY_EDITOR
            if (_dicForCount.Count != _keyList.Count)
            {
                _dicForCount = new Dictionary<TKey, TVal>();
                for (int i = 0; i < _keyList.Count; i++)
                {
                    var key = _keyList[i];
                    var val = _valList[i];
                    _dicForCount.Add(key, val);
                }
            }
#endif
            return _dicForCount;
        }

        public bool TryAdd(TKey key, TVal val)
        {
            if (this.IsDefaultOrEmpty(key)) return false;
            CheckFitDic();
            if (ContainsKey(key))
            {
                return false;
            }
            _keyList.Add(key);
            _valList.Add(val);
            _dicForCount.Add(key, val);
            return true;
        }

        public void Add(TKey key, TVal val)
        {
            if (this.IsDefaultOrEmpty(key)) return;
            CheckFitDic();
            _keyList.Add(key);
            _valList.Add(val);
            _dicForCount.Add(key, val);
        }

        public bool TryGetValue(TKey key, out TVal val)
        {
            val = default(TVal);
            if (this.IsDefaultOrEmpty(key)) return false;
            CheckFitDic();
            return _dicForCount.TryGetValue(key, out val);
        }

        public void Clear()
        {
            _dicForCount?.Clear();
            _keyList.Clear();
            _valList.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _keyList.Contains(key);
        }

        public bool ContainsValue(TVal val)
        {
            return _valList.Contains(val);
        }

        public void UpdateValue(TKey key, TVal val)
        {
            if (this.IsDefaultOrEmpty(key)) return;
            if (ContainsKey(key))
            {
                Remove(key);
            }
            Add(key, val);
        }

        public bool Remove(TKey key)
        {
            return Remove(key, out var val);
        }
        
        public bool Remove(TKey key, out TVal val)
        {
            val = default(TVal);
            if (this.IsDefaultOrEmpty(key)) return false;
            CheckFitDic();
            _dicForCount.Remove(key, out val);
            _keyList.Remove(key);
            _valList.Remove(val);
            return true;
        }
    }
}