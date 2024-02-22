/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alchemy
{
    public class AL_ResMgr
    {
        private static Al_ResLoaderMode _curMode = Al_ResLoaderMode.aa;
        private static AL_BaseLoader _curLoader;

        public static AL_BaseLoader CurLoader
        {
            get
            {
                if (_curLoader != null) return _curLoader;
                switch (_curMode)
                {
                    case Al_ResLoaderMode.aa:
                    case Al_ResLoaderMode.ab:
                        _curLoader = new Al_AaLoader();
                        break;
                }
                return _curLoader;
            }
        }

        public static void InitAsync(Action<bool> onComplete)
        {
            CurLoader.InitAsync(onComplete);
        }

        public static void CheckCDNSizeAsync(List<string> dataKeyList, Action<double, bool> onComplete)
        {
            UnityEngine.Debug.Log("CheckCDNSizeAsync start");
            CurLoader.CheckCDNSizeAsync(dataKeyList, onComplete);
        }

        // public static void DownloadAsync(string dataKey, Action onComplete, Action onFailed = null)
        // {
        //     CurLoader.CDNDownloadAsync(dataKey, onComplete, onFailed);
        // }
        //
        // public static void DownloadAsync(List<string> dataKeyList, Action onComplete, Action onFailed = null)
        // {
        //     CurLoader.CDNDownloadAsync(dataKeyList, onComplete, onFailed);
        // }
        
        public static void PreLoadAsync(List<string> resList, Action<bool> onComplete)
        {
            UnityEngine.Debug.Log("PreLoadAsync start");
            CurLoader.PreLoadAsync<Texture2D>(resList, onComplete);
        }

        public static void LoadAssetAsync<T>(string key, Action<T, bool> onComplete)
        {
            CurLoader.LoadAssetAsync<T>(key, onComplete);
        }
    }

    public enum Al_ResLoaderMode
    {
        aa,
        ab
    }
}