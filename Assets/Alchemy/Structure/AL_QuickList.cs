using System.Collections.Generic;
using UnityEditor;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 复杂实现不要用这个(只用到以下实现接口的才可以使用）
    /// Contains\Add\Remove\Clear
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AL_QuickList<T>:List<T>
    {
        private Dictionary<T, bool> _containItems;

        public AL_QuickList():base()
        {
            _containItems = new Dictionary<T, bool>();
        }
        
        public new bool Contains(T item)
        {
            return _containItems.ContainsKey(item);
        }

        public new void Add(T item)
        {
            base.Add(item);
            _containItems[item] = true;
        }

        public new bool Remove(T item)
        {
            var succ = base.Remove(item);
            if (succ) _containItems.Remove(item);
            return succ;
        }

        public new void Clear()
        {
            base.Clear();
            _containItems.Clear();
        }

        
    }
}