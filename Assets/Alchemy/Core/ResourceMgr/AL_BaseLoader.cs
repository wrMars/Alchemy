/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using System.Collections.Generic;

namespace Alchemy
{
    public class AL_BaseLoader
    {
        public virtual void InitAsync(Action<bool> onComplete){}

        public virtual void CDNDownloadAsync(string dataKey, Action<bool> onComplete){}

        public virtual void CDNDownloadAsync(List<string> dataKeyList, Action<bool> onComplete){}

        public virtual void PreLoadAsync<T>(List<string> resList, Action<bool> onComplete){}

        public virtual void LoadAssetAsync<T>(string key, Action<T, bool> onComplete){}

        public virtual T LoadAsset<T>(string key)
        {
            return default(T);
            
        }

        public virtual void CheckCDNSizeAsync(List<string> dataKeyList, Action<double, bool> onComplete){}

        public virtual bool HasAddressableKey<T>(object key)
        {
            return false;
        }
    }
}