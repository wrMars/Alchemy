using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    [Serializable]
    public class AL_SerializableHashSet<T>
    {
        [SerializeField]
        private List<T> _dataList;

        public List<T> DataList => _dataList;

        public int Count => DataList.Count;

        public AL_SerializableHashSet(List<T> list = null)
        {
            _dataList = list ?? new List<T>();
        }

        public AL_SerializableHashSet(IEnumerable<T> collection)
        {
            _dataList = collection?.ToList() ?? new List<T>();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var data in collection)
            {
                Add(data);
            }
        }
        
        public void AddRange(AL_SerializableHashSet<T> hash)
        {
            foreach (var data in hash.DataList)
            {
                Add(data);
            }
        }

        public HashSet<T> ToHashSet()
        {
            return new HashSet<T>(DataList);
        }

        public bool Equals(AL_SerializableHashSet<T> data)
        {
            if (data == null) return false;
            var selfHash = ToHashSet();
            var tarHash = data.ToHashSet();
            return selfHash.Equals(tarHash);
        }

        public AL_SerializableHashSet<T> ExceptWith(AL_SerializableHashSet<T> data)
        {
            if (data == null) return this;
            var selfHash = ToHashSet();
            var tarHash = data.ToHashSet();
            selfHash.ExceptWith(tarHash);
            return new AL_SerializableHashSet<T>(selfHash);
        }

        public bool Contains(T data)
        {
            return DataList.Contains(data);
        }

        public void Add(T data)
        {
            if (Contains(data)) return;
            DataList.Add(data);
        }

        public void Remove(T data)
        {
            if (Contains(data))
            {
                DataList.Remove(data);
            }
        }
    }
}