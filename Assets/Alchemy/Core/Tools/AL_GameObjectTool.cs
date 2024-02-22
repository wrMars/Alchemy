/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace Alchemy
{
    public class AL_GameObjectTool
    {
        public static string GetInheritStr(Transform transform, string output = "")
        {
            output = output.Insert(0, transform.gameObject.name + (output == ""? "":" > "));
            if (transform.parent != null)
            { 
                return GetInheritStr(transform.parent, output);
            }
            else
            {
                return output;
            }
        }

        public static Transform GetOldestTran(Transform transform)
        {
            if (transform != null && transform.parent != null)
            {
                return GetOldestTran(transform.parent);
            }
            else
            {
                return transform;
            }
        }
        
        private static bool CheckObjHadNull(object go1, object go2, out bool both)
        {
            both = go1 == null && go2 == null;
            return go1 == null || go2 == null;
        }
        
        public static bool IsObjEquals(GameObject go1, GameObject go2, out AL_GameObjectDiffInfo from1to2, out AL_GameObjectDiffInfo from2to1)
        {
            from1to2 = from2to1 = null;
            if (CheckObjHadNull(go1, go2, out bool both))
            {
                return both;
            }
            
            // if (go1.name != go2.name) return false;
            // if (go1.transform.position != go2.transform.position) return false;
            // if (!IsComponentsEquals(go1, go2, )) return false;
            // if (!IsChildrenObjEquals(go1, go2)) return false;
            
            var pos1 = go1.transform.position;
            var pos2 = go2.transform.position;
            from1to2 = new AL_GameObjectDiffInfo();
            from1to2.SelfObj = go1;
            from1to2.GoalObj = go2;
            from1to2.DiffName = go2.name;
            from1to2.DiffPos = pos2;
            
            from2to1 = new AL_GameObjectDiffInfo();
            from2to1.SelfObj = go2;
            from2to1.GoalObj = go1;
            from2to1.DiffName = go1.name;
            from2to1.DiffPos = pos1;

            
            var e1 = (go1.transform.parent == null && go2.transform.parent == null) ? true : go1.name == go2.name;
            var e2 = pos1 == pos2;
            var e3 = IsComponentsEquals(go1, go2, from1to2, from2to1);
            var e4 = IsChildrenObjEquals(go1, go2, from1to2, from2to1);

            bool equal = e1 && e2 && e3 && e4;
            from1to2.IsEqual = from2to1.IsEqual = equal;
            return equal;
        }

        public static bool IsChildrenObjEquals(GameObject gameobj1, GameObject gameobj2, AL_GameObjectDiffInfo from1to2, AL_GameObjectDiffInfo from2to1)
        {
            if (CheckObjHadNull(gameobj1, gameobj2, out bool both))
            {
                return both;
            }

            Func<GameObject, GameObject, AL_GameObjectDiffInfo, AL_GameObjectDiffInfo, bool> func = (go1, go2, f1to2, f2to1) =>
            {
                bool equal = go1.transform.childCount == go2.transform.childCount;
                for (int i = 0; i < go1.transform.childCount; i++)
                {
                    bool curEqual = false;
                    bool sameName = false;
                    AL_GameObjectDiffInfo out1 = null;
                    AL_GameObjectDiffInfo out2 = null;
                    GameObject childObject1 = go1.transform.GetChild(i).gameObject;
                    for (int j = 0; j < go2.transform.childCount; j++)
                    {
                        GameObject childObject2 = go2.transform.GetChild(j).gameObject;
                        if (childObject1.name == childObject2.name)
                        {
                            sameName = true;
                            if (IsObjEquals(childObject1, childObject2, out out1, out out2))
                            {
                                curEqual = true;
                                break;
                            }
                            f1to2.AddDiffChildObj(out1);
                            f2to1.AddDiffChildObj(out2);
                        }
                    }

                    if (!curEqual)
                    {
                        if (!sameName)
                        {
                            f1to2.AddSurplusObjs(childObject1);
                            f2to1.AddLackObjs(childObject1);
                        }
                        UnityEngine.Debug.LogWarning($"对比子节点：{GetInheritStr(childObject1.transform)} 在 {GetInheritStr(go2.transform)} 中找不到相等的子节点");
                        equal = false;
                    }
                }

                return equal;
            };
            func(gameobj1, gameobj2, from1to2, from2to1);
            return func(gameobj2, gameobj1, from2to1, from1to2);;
        }

        public static bool IsComponentsEquals(GameObject go1, GameObject go2, AL_GameObjectDiffInfo from1to2, AL_GameObjectDiffInfo from2to1)
        {
            if (CheckObjHadNull(go1, go2, out bool both))
            {
                return both;
            }
            List<Component> monos1 = go1.GetComponents<Component>().ToList();
            List<Component> monos2 = go2.GetComponents<Component>().ToList();
            
            Func<List<Component>, List<Component>, AL_GameObjectDiffInfo, AL_GameObjectDiffInfo, bool> func = (list1, list2, f1to2, f2to1) =>
            {
                bool equal = list1.Count == list2.Count;
                foreach (var c1 in list1)
                {
                    bool curEqual = false;
                    bool isSameType = false;
                    Component tempc2 = null;
                    foreach (var c2 in list2)
                    {
                        if (IsComponentEquals(c1, c2, out var isSame))
                        {
                            curEqual = true;
                        }

                        if (isSame)
                        {
                            isSameType = true;
                            tempc2 = c2;
                        }
                        if (curEqual) break;
                    }
                    if (!curEqual)
                    {
                        if (isSameType)
                        {
                            f1to2?.AddDiffComponent(c1, tempc2);
                            f2to1?.AddDiffComponent(tempc2, c1);
                        }
                        else
                        {
                            f1to2?.AddSurplusComponent(c1);
                            f2to1?.AddLackComponent(c1);
                        }
                        UnityEngine.Debug.LogWarning($"对比脚本：{GetInheritStr(go1.transform)}.{c1.name} 在 {GetInheritStr(go2.transform)} 中找不到相等的");
                        equal = false;
                    }
                }
                return equal;
            };
            func(monos1, monos2, from1to2, from2to1);
            return func(monos2, monos1, from2to1, from1to2);
        }
        
        public static bool IsComponentEquals(Component obj1, Component obj2, out bool isSameType)
        {
            isSameType = false;
            if (CheckObjHadNull(obj1, obj2, out bool both))
            {
                return both;
            }
            Type type = obj1.GetType();
            isSameType = type == obj2.GetType();
            if (!isSameType) return false;
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                object value1 = field.GetValue(obj1);
                object value2 = field.GetValue(obj2);

                if (!Equals(value1, value2))
                {
                    UnityEngine.Debug.LogWarning($"脚本内参数对比：{obj1.name}.{field.Name} 不相等");
                    return false;
                }
            }
            return true;
        }
    }

    public class AL_GameObjectDiffInfo
    {
        public GameObject SelfObj;
        public GameObject GoalObj;//目标
        public bool IsEqual;
        public string DiffName;
        public Vector3 DiffPos;
        public List<Component> SurplusComponents;
        public List<Component> LackComponents;//这里装的是别人的
        public List<Component> DiffComponents_Other;//这里装的是别人的
        public List<Component> DiffComponents_Self;//这里装的是自己的
        public List<GameObject> SurplusObjs;
        public List<GameObject> LackObjs;
        public List<AL_GameObjectDiffInfo> DiffChildObjs;
        private string _space = "    ";

        private string GetDeepSpace(int deep)
        {
            var re = "";
            for (int i = 0; i < deep; i++)
            {
                re += _space;
            }
            return re;
        }
        
        public string ToString(int deep = 0)
        {
            var str = $"{GetDeepSpace(deep)}【startObj】{AL_GameObjectTool.GetInheritStr(SelfObj.transform)} <=> {AL_GameObjectTool.GetInheritStr(GoalObj.transform)}\n";
            
            if (SurplusComponents?.Count > 0)
            {
                str += $"{GetDeepSpace(deep)}{_space}SurplusComponents【{SurplusComponents?.Count}】:{string.Join(",", SurplusComponents.Select(com => AL_GameObjectTool.GetInheritStr(com.transform)).ToList())}\n";
            }

            if (LackComponents?.Count > 0)
            {
                str +=$"{GetDeepSpace(deep)}{_space}LackComponents【{LackComponents.Count}】:{string.Join(",", LackComponents.Select(com => AL_GameObjectTool.GetInheritStr(com.transform)).ToList())}\n";
            }
            
            if (DiffComponents_Self?.Count > 0)
            {
                str += $"{GetDeepSpace(deep)}{_space}DiffComponents_Self【{DiffComponents_Self.Count}】:{string.Join(",", DiffComponents_Self.Select(com => AL_GameObjectTool.GetInheritStr(com.transform)).ToList())}\n";
            }
            if (SurplusObjs?.Count > 0)
            {
                str += $"{GetDeepSpace(deep)}{_space}SurplusObjs【{SurplusObjs.Count}】:{string.Join(",", SurplusObjs.Select(com => AL_GameObjectTool.GetInheritStr(com.transform)).ToList())}\n";
            }
            if (LackObjs?.Count > 0)
            {
                str += $"{GetDeepSpace(deep)}{_space}LackObjs【{LackObjs.Count}】:{string.Join(",", LackObjs.Select(com => AL_GameObjectTool.GetInheritStr(com.transform)).ToList())}\n";
            }

            if (DiffChildObjs?.Count > 0)
            {
                for (int i = 0; i < DiffChildObjs.Count; i++)
                {
                    str += $"{GetDeepSpace(deep)}{_space}DiffChildObjs【{i + 1}/{DiffChildObjs.Count}】\n{DiffChildObjs[i].ToString(deep + 1)}";
                }
            }
            str += $"{GetDeepSpace(deep)}【endObj】{SelfObj.name} <=> {GoalObj.name}\n";
            return str;
        }
        
        public void AddSurplusComponent(Component com)
        {
            SurplusComponents ??= new List<Component>();
            if (SurplusComponents.Contains(com)) return;
            SurplusComponents.Add(com);
        }

        public void AddDiffComponent(Component s, Component other)
        {
            DiffComponents_Self ??= new List<Component>();
            if (DiffComponents_Self.Contains(s)) return;
            DiffComponents_Self.Add(s);
            
            DiffComponents_Other ??= new List<Component>();
            if (DiffComponents_Other.Contains(other)) return;
            DiffComponents_Other.Add(other);
        }
        
        public void AddLackComponent(Component com)
        {
            LackComponents ??= new List<Component>();
            if (LackComponents.Contains(com)) return;
            LackComponents.Add(com);
        }

        public void AddSurplusObjs(GameObject obj)
        {
            SurplusObjs ??= new List<GameObject>();
            if (SurplusObjs.Contains(obj)) return;
            SurplusObjs.Add(obj);
        }
        
        public void AddLackObjs(GameObject obj)
        {
            LackObjs ??= new List<GameObject>();
            if (LackObjs.Contains(obj)) return;
            LackObjs.Add(obj);
        }

        public void AddDiffChildObj(AL_GameObjectDiffInfo info)
        {
            DiffChildObjs ??= new List<AL_GameObjectDiffInfo>();
            if (DiffChildObjs.Find(obj=> obj.SelfObj == info.SelfObj && obj.GoalObj == info.GoalObj) != null) return;
            DiffChildObjs.Add(info);
        }

        private string _prefabLogo = "_Diff";
#if UNITY_EDITOR
        public void SaveInfo()
        {
            var path = AssetDatabase.GetAssetPath(SelfObj);
            var go = CreateInfoGo();
            path = path.Replace(SelfObj.name, go.name);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Object.DestroyImmediate(go);
        }
        
        private GameObject CreateInfoGo(Transform parent = null)
        {
            GameObject go = null;
            if (parent == null)
            {
                go = new GameObject($"{SelfObj.name}{_prefabLogo}");
            }
            else
            {
                go = new GameObject($"{SelfObj.name}");
                go.transform.SetParent(parent);
            }

            CreateComponentsGameObject(SurplusComponents, nameof(SurplusComponents), go.transform);
            CreateComponentsGameObject(LackComponents, nameof(LackComponents), go.transform);
            CreateComponentsGameObject(DiffComponents_Other, nameof(DiffComponents_Other), go.transform);
            CreateComponentsGameObject(DiffComponents_Self, nameof(DiffComponents_Self), go.transform);

            CreateGameObject(SurplusObjs, nameof(SurplusObjs), go.transform);
            CreateGameObject(LackObjs, nameof(LackObjs), go.transform);

            if (DiffChildObjs?.Count > 0)
            {
                foreach (var info in DiffChildObjs)
                {
                    info.CreateInfoGo(go.transform);
                } 
            }
            return go;
        }

        private GameObject CreateComponentsGameObject(List<Component> components, string goName, Transform parent)
        {
            if (components?.Count > 0)
            {
                GameObject go = new GameObject(goName);
                foreach (var com in components)
                {
                    var inCom = go.AddComponent(com.GetType());
                    AL_ReflectTool.CopyObject(com, inCom);
                }
                go.name = goName;
                go.transform.SetParent(parent);
                return go;
            }
            return null;
        }

        private GameObject CreateGameObject(List<GameObject> gos, string goName, Transform parent)
        {
            if (gos?.Count > 0)
            {
                GameObject reGo = new GameObject(goName);
                foreach (var go in gos)
                {
                    GameObject inGo = GameObject.Instantiate(go);
                    inGo.transform.SetParent(reGo.transform);
                }
                reGo.name = goName;
                reGo.transform.SetParent(parent);
                return reGo;
            }
            return null;
        }
#endif
        
    }
}