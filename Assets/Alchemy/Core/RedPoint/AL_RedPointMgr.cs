using System.Collections.Generic;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

namespace Alchemy.RedPoint
{
    /// <summary>
    /// 使用此红点管理类，需要注意game object被销毁后失效的情况，即彼时需要重新添加红点事件（字符串为key）
    /// 【注意】组件复用的时候，留意删除原先的红点监听并重新添加;同一gameobject可监听多个红点事件
    /// 可以视情况灵活使用，复杂情况可以使用诸如 xxx_1、xxx_2、xxx_3这样拆分多种红点key来处理逻辑
    /// 【支持先触发事件再注册的情况】
    /// </summary>
    public class AL_RedPointMgr
    {
        private static HashSet<string> _nowActiveKeys = new HashSet<string>();
        private static Dictionary<GameObject, List<string>> _dicGoShowRpEvents = new Dictionary<GameObject, List<string>>();//当前go身上挂的显示事件
        
        private static Dictionary<string, List<GameObject>> _dicLogo2Go = new Dictionary<string, List<GameObject>>();//放不需要特殊检查的红点go

        //移除该GO某个红点监听
        public static void Remove(GameObject go, string logo, bool checkShow = true)
        {
            if (go == null || string.IsNullOrEmpty(logo)) return;
            RemoveGoInDic(_dicLogo2Go, go, logo);
            if (_dicGoShowRpEvents.TryGetValue(go, out var list) && list?.Count > 0)
            {
                list.Remove(logo);
            }
            if (checkShow) CheckGoShow(go);
        }

        //移除所有该GO的红点监听
        public static void Remove(GameObject go)
        {
            if (go == null) return;
            RemoveGoInDic(_dicLogo2Go, go);
            _dicGoShowRpEvents.Remove(go);
        }

        private static void RemoveGoInDic(Dictionary<string, List<GameObject>> dic, GameObject go, string logo = "")
        {
            if (go == null || dic == null) return;
            bool checkLogo = !string.IsNullOrEmpty(logo);
            foreach (var kv in dic)
            {
                if (checkLogo && kv.Key != logo) continue;
                if (kv.Value != null)
                {
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        if (kv.Value[i] == go)
                        {
                            kv.Value.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        public static void RemoveAndAdd(GameObject go, string rpLogo)
        {
            Remove(go, rpLogo, false);
            TryAdd(go, rpLogo);
        }
        
        private static void CheckGoShow(GameObject go)
        {
            if (go == null) return;
            bool show = _dicGoShowRpEvents.TryGetValue(go, out var list) && list?.Count > 0;
            _UpdateGoActive(go, show);
        }

        //刚注册时候检查是否已经有预先触发的红点事件了
        private static void CheckGoShowWhenAdd(GameObject go, string logo)
        {
            if (go == null || string.IsNullOrEmpty(logo)) return;
            bool show = _nowActiveKeys.Contains(logo);
            if (show)
            {
                TryAddEventInGoAndShow(go, logo);
            }
            else
            {
                //此前没有进观察区，所以直接设置不显示即可
                go.SetActive(false);
            }
        }
        
        public static void TryAdd(GameObject go, string logo)
        {
            if (go == null || string.IsNullOrEmpty(logo)) return;
            TryAddInDicList(_dicLogo2Go, logo, go);
            CheckGoShowWhenAdd(go, logo);
        }

        private static void ActionNormal(string logo, bool show)
        {
            if (_dicLogo2Go.TryGetValue(logo, out var gos))
            {
                _tempRemoveIndex.Clear();
                if (gos?.Count > 0)
                {
                    for (int i = 0; i < gos.Count; i++)
                    {
                        var go = gos[i];
                        if (go == null) _tempRemoveIndex.Add(i);
                        if (show)
                        {
                            TryAddEventInGoAndShow(go, logo);
                        }
                        else
                        {
                            TryRemoveEventInGoAndShow(go, logo);
                        }
                    }
                }
                if (_tempRemoveIndex.Count > 0)
                {
                    _tempRemoveIndex.ForEach(val=> gos.RemoveAt(val));
                    AL_FileLog.LogErrorLater($"红点监听的gameobject有被销毁的，留意重新注册");
                }
            }
        }
        private static List<int> _tempRemoveIndex = new List<int>();
        
        public static void Action(string logo, bool show)
        {
            if (show)
            {
                _nowActiveKeys.Add(logo);
            }
            else
            {
                _nowActiveKeys.Remove(logo);
            }
            ActionNormal(logo, show);
        }
        
        private static void _UpdateGoActive(GameObject go, bool show)
        {
            if (go == null) return;
            if (go.activeSelf != show) go.SetActive(show);
        }
        
        private static void TryAddEventInGoAndShow(GameObject go, string logo)
        {
            if (go == null || string.IsNullOrEmpty(logo)) return;
            TryAddInDicList(_dicGoShowRpEvents, go, logo);
            _UpdateGoActive(go, true);
        }

        private static void TryRemoveEventInGoAndShow(GameObject go, string logo)
        {
            if (go == null || string.IsNullOrEmpty(logo)) return;
            if (_dicGoShowRpEvents.TryGetValue(go, out var list) && list?.Count > 0)
            {
                list.Remove(logo);
            }
            _UpdateGoActive(go, (list?.Count ?? 0) != 0);
        }

        private static void TryAddInDicList<TDickey, TDicval, TList>(Dictionary<TDickey, TDicval> dic, TDickey key,
            TList listval) where TDicval : IList<TList>, new()
        {
            if (dic == null) return;
            if (!dic.TryGetValue(key, out TDicval list))
            {
                list = new TDicval();
                dic.Add(key, list);
                list.Add(listval);
            }
            else
            {
                if (list.Contains(listval)) return;
                list.Add(listval);
            }
        }
    }
}