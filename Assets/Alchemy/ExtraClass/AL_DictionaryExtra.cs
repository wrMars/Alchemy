using System.Collections.Generic;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy.ExtraClass
{
    public static class AL_DictionaryExtra
    {
        public static bool Remove<TKey, TVal>(this Dictionary<TKey, TVal> dic, TKey key, out TVal val)
        {
            if (dic.TryGetValue(key, out val)) return false;
            return dic.Remove(key);
        }
    }
}