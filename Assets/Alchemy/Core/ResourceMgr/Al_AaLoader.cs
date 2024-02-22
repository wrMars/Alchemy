/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Alchemy
{
    public class Al_AaLoader:AL_BaseLoader
    {
        public override void InitAsync(Action<bool> onComplete)
        {
            AsyncOperationHandle<IResourceLocator> initializeAsync = Addressables.InitializeAsync();
            initializeAsync.Completed += handle =>
            {
                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                checkHandle.Completed += listHandle =>
                {
                    if (checkHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (checkHandle.Result.Count > 0)
                        {
                            AL_FileLog.LogError($"Catalog Find out {checkHandle.Result.Count} Updates");
                            UnityEngine.Debug.Log("checkHandle.Result:" + string.Join(",", checkHandle.Result));
                            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
                            updateHandle.Completed += h_updateCatalogs =>
                            {
                                AL_FileLog.LogError("Catalog Update complete ");
                                onComplete?.Invoke(h_updateCatalogs.Status == AsyncOperationStatus.Succeeded);
                                Addressables.Release(updateHandle);
                            };
                        }
                        else
                        {
                            AL_FileLog.LogError("Catalog No updates ");
                            onComplete?.Invoke(true);
                        }
                    }
                    else
                    {
                        AL_FileLog.LogError("Catalog Failed to connect to server ");
                        onComplete?.Invoke(false);
                    }
                    Addressables.Release(checkHandle);
                };
                Addressables.Release(initializeAsync);
            };
        }
        
        public static double ByteToMB(long size)
        {
            return ((double)size / 1024f) / 1024f;
        }

        public override void CheckCDNSizeAsync(List<string> dataKeyList, Action<double, bool> onComplete)
        {
            AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(dataKeyList);
            handle.Completed += operationHandle =>
            {
                double downloadSize = 0;
                bool checkSucceeded = false;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    downloadSize = ByteToMB(handle.Result);
                    checkSucceeded = true;
                    AL_FileLog.LogError("[CDN] GetDownloadSizeAsync succ : " + downloadSize);
                }
                else
                {
                    AL_FileLog.LogError("[CDN] GetDownloadSizeAsync Fail DebugName : " + handle.DebugName);
                    AL_FileLog.LogError("[CDN] GetDownloadSizeAsync Fail Exception : " + handle.OperationException.Message);
                    checkSucceeded = false;
                }
                if (checkSucceeded)
                {
                    onComplete?.Invoke(downloadSize, true);
                }
                else
                {
                    onComplete?.Invoke(0, false);
                }
                Addressables.Release(handle);
            };
        }
        
        private bool IsCDNDownloading(string datakey)
        {
            return _cdnDownloadList.Contains(datakey);
        }

        private List<string> _cdnDownloadList = new List<string>();
        public override void CDNDownloadAsync(string dataKey, Action<bool> onComplete)
        {
            if (IsCDNDownloading(dataKey))
                return;
            _cdnDownloadList.Add(dataKey);
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(dataKey);
            handle.Completed += operationHandle =>
            {
                _cdnDownloadList.Remove(dataKey);
                CheckDoReadyHandle(handle, onComplete);
            };
        }

        public override void CDNDownloadAsync(List<string> dataKeyList, Action<bool> onComplete)
        {
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(dataKeyList, Addressables.MergeMode.Union);
            handle.Completed += operationHandle =>
            {
                CheckDoReadyHandle(handle, onComplete);
            };
        }
        
        private void CheckDoReadyHandle(AsyncOperationHandle handle, Action<bool> onComplete)
        {
            onComplete?.Invoke(handle.Status == AsyncOperationStatus.Succeeded);
            Addressables.Release(handle);
        }
        
        public override void PreLoadAsync<T>(List<string> resList, Action<bool> onComplete)
        {
            Addressables.LoadAssetsAsync<T>(resList, null, Addressables.MergeMode.Union).Completed += handle =>
            {
                onComplete?.Invoke(handle.Status == AsyncOperationStatus.Succeeded);
            };
        }
        
        public override bool HasAddressableKey<T>(object key)
        {
            if (key is IKeyEvaluator)
                key =  (key as IKeyEvaluator).RuntimeKey;

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(T), out var locs))
                {
                    return true;
                }
            }
            return false;
        }

        public override void LoadAssetAsync<T>(string key, Action<T, bool> onComplete)
        {
            var handle =  Addressables.LoadAssetAsync<T>(key);
            handle.Completed += operationHandle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    onComplete?.Invoke(handle.Result, true);
                }
                else
                {
                    onComplete?.Invoke(handle.Result, false);
                    AL_FileLog.LogError($"[LOADASSET FAIL] {key} is null");
                }
            };
        }
        
        private void CheckLocal(string key, Action<bool> onComplete)
        {
            var local = Addressables.LoadResourceLocationsAsync(key);
            local.Completed += handle =>
            {
                if (local.Result.Count == 0)
                {
                    AL_FileLog.LogError($"Instantiate CheckLocal  本地没有资源：{key}; status:{local.Status}");
                }
                onComplete?.Invoke(local.Result.Count == 0);
                Addressables.Release(local);
            };
        }

        public void InstantiateAsync<T>(string key, Transform parent, Action<T, bool> onComplete)
        {
            Addressables.InstantiateAsync(key, parent).Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded && operationHandle.Result != null)
                {
                    onComplete?.Invoke(operationHandle.Result.GetComponent<T>(), true);
                }
                else
                {
                    AL_FileLog.LogError($"[LOADASSET FAIL] {key} is null");
                    onComplete?.Invoke(default, false);
                }
            };
        }
        
        public void LoadSceneAsync(string key, Action<SceneInstance> onComplete = null)
        {
            Addressables.LoadSceneAsync(key, LoadSceneMode.Additive).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onComplete?.Invoke(handle.Result);
                }
            };
        }

        public void UnloadSceneAsync(SceneInstance sceneInstance, Action onComplete = null)
        {
            Addressables.UnloadSceneAsync(sceneInstance).Completed += (handle) =>
            {
                onComplete?.Invoke();
            };
        }

        #region 同步
        /// <summary>
        /// 确保已经下载好的情况下才使用
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T LoadAsset<T>(string key)
        {
            var tempT = Addressables.LoadAssetAsync<T>(key).Result;
            if (tempT == null)
            {
                AL_FileLog.LogError($"[LOADASSET FAIL] {key} is null");
            }
            return tempT;
        }

        public T Instantiate<T>(string key, Transform parent)
        {
            GameObject tempT = Addressables.InstantiateAsync(key, parent).Result;
            if(tempT == null)
            {
                AL_FileLog.LogError($"{key} is null");
            }

            return tempT.GetComponent<T>();
        }
        
        
        
        public void ReleaseAsset<T>(T gameObject)
        {
            Addressables.Release(gameObject);
        }

        public void ReleaseInstance(GameObject gameObject)
        {
            Addressables.ReleaseInstance(gameObject);
        }
        #endregion
        
        
        
    }
}