using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_ReferenceHelperWindow : AlchemyBaseEditorWindow<AL_ReferenceHelperWindow,ReferenceHelperData>
    {
        public const string ModuleSign = "ReferenceHelperWindow";
        public override string ModuleName { get; set; } = ModuleSign;

        protected override void StaticEventReAdd()
        {
            StaticEventReset();
            Selection.selectionChanged += _OnSelectionChange;
        }

        protected override void StaticEventReset()
        {
            Selection.selectionChanged -= _OnSelectionChange;
        }

        public void ReInitReference()
        {
            var arr = AssetDatabase.GetAllAssetPaths().ToList().FindAll((path) => path.IndexOf("Assets/") == 0);
            ModuleData?.ReInit(arr);
        }
        
        public ReferenceRelationshipData GetReferenceData(string assetPath)
        {
            if (AL_ReferenceHelper.IsIllegal(assetPath)) return null;
            if (ModuleData != null)
            {
                ModuleData.DicRelationship.TryGetValue(assetPath, out var referenceData);
                return referenceData;
            }
            return null;
        }
        
        
        [MenuItem("Alchemy/引用&依赖工具")]
        private static void ShowWindow()
        {
            OpenWindow("引用&依赖工具");
        }
        
        void OnOperationPartUpdate()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重建关系", GUILayout.Height(30)))
            {
                ReInitReference();
                Debug.LogError("重建引用&依赖关系结束");
            }
            EditorGUILayout.EndHorizontal();
        }

        private string curTip = "";
        void OnTipPartUpdate()
        {
            EditorGUILayout.LabelField("使用说明：");
            EditorGUILayout.LabelField("1、在重建关系后，可选中对应的obj来查看其 引用&依赖；");
            EditorGUILayout.LabelField("2、在hierarchy界面，不需要重建关系，可选中对应的obj来查看其被引用信息；");
            EditorGUILayout.LabelField("*************************************************************************");
            EditorGUILayout.BeginHorizontal();
            curTip = ModuleData?.DicRelationship.Count <= 0 ? "当前无全项目的【引用&依赖关系】数据，需要重建（Hierarchy界面依赖查看功能可正常使用）" : "当前有全项目的【引用&依赖关系】数据";
            EditorGUILayout.LabelField(string.Format("Tip:{0}", curTip));
            EditorGUILayout.EndHorizontal();
        }
        
        private string curSelectPath;
        void OnChoosePartUpdate()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            curSelectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUILayout.ObjectField("当前选中:", Selection.activeObject, typeof(Object), false);
            EditorGUILayout.EndHorizontal();
        }

        private Vector2 scrollPos1 = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private Vector2 scrollPos3 = Vector2.zero;
        void OnDependenciesPartUpdate()
        {
            EditorGUILayout.Space();
            var data = GetReferenceData(curSelectPath);
            if ((data?.DirectlyDependencies.Count ?? 0) <= 0) return;
            EditorGUILayout.LabelField(string.Format("依赖({0}):", data.DirectlyDependencies.Count));
            
            scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1);
            foreach (var depenPath in data.DirectlyDependencies.DataList)
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(depenPath);
                EditorGUILayout.ObjectField(obj, typeof(Object), false);
            }
            EditorGUILayout.EndScrollView();
        }

        void OnReferencesPartUpdate()
        {
            EditorGUILayout.Space();
            var data = GetReferenceData(curSelectPath);
            if ((data?.DirectlyReferences.Count ?? 0) <= 0) return;
            EditorGUILayout.LabelField(string.Format("被引用({0}):", data.DirectlyReferences.Count));
            
            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);
            foreach (var refPath in data.DirectlyReferences.DataList)
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(refPath);
                EditorGUILayout.ObjectField(obj, typeof(Object), false);
            }
            EditorGUILayout.EndScrollView();
        }

        private Component[] _selectComponentsHierarchyPart;
        private MonoBehaviour[] _tranHierarchyPartAllMono;
        private Dictionary<GameObject, List<string>> _dicHierarchyPart;

        private void CheckAddHierarchyReferenceMsg(GameObject go, string msg)
        {
            if (go == null) return;
            if (!_dicHierarchyPart.ContainsKey(go))
            {
                _dicHierarchyPart[go] = new List<string>();
            }

            var arr = _dicHierarchyPart[go.gameObject];
            if (!arr.Contains(msg))
            {
                _dicHierarchyPart[go.gameObject].Add(msg);
            }
        }

        private void _OnSelectionChange()
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject selectGo)
            {
                _selectComponentsHierarchyPart = selectGo.transform.GetComponents<Component>();
                var tran = AL_GameObjectTool.GetOldestTran(selectGo.transform);
                _tranHierarchyPartAllMono = tran.GetComponentsInChildren<MonoBehaviour>();
                FieldInfo[] fields;
                _dicHierarchyPart ??= new Dictionary<GameObject, List<string>>();
                _dicHierarchyPart.Clear();
                string inheritStr;

                bool checkOneComponent(object fieldVal, out Component com)
                {
                    com = null;
                    foreach (var selectComponent in _selectComponentsHierarchyPart)
                    {
                        if (ReferenceEquals(fieldVal, selectComponent))
                        {
                            com = selectComponent;
                            return true;
                        }
                    }
                    return false;
                }
                
                for (int i = 0; i < _tranHierarchyPartAllMono.Length; i++)
                {
                    var mono = _tranHierarchyPartAllMono[i];
                    if (ReferenceEquals(mono.gameObject, selectGo)) continue;
                    Type targetType = mono.GetType();
                    // fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    fields = AL_ReflectTool.GetMonoSerializeField(targetType);
                    foreach (FieldInfo field in fields)
                    {
                        var val = field.GetValue(mono);
                        if (field.FieldType.IsArrayOrList(out Type elementType) && (elementType.IsSubclassOf(typeof(Component)) || elementType == typeof(GameObject)))
                        {
                            var list = (IList) val;
                            foreach (var com in list)
                            {
                                if (elementType.IsSubclassOf(typeof(Component)) && checkOneComponent(com, out Component selectComponent) && selectComponent != null)
                                {
                                    inheritStr = $"{AL_GameObjectTool.GetInheritStr(mono.transform)} : {mono.GetType()} . {field.Name} => {selectGo.name} : {selectComponent.GetType()}";
                                    CheckAddHierarchyReferenceMsg(mono.gameObject, inheritStr);
                                } else if (elementType == typeof(GameObject) && ReferenceEquals(com, selectGo))
                                {
                                    inheritStr = $"{AL_GameObjectTool.GetInheritStr(mono.transform)} : {mono.GetType()} . {field.Name} => {selectGo.name}";
                                    CheckAddHierarchyReferenceMsg(mono.gameObject, inheritStr);
                                }
                            }
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(Component)) && checkOneComponent(val, out Component selectComponent) && selectComponent != null)
                        {
                            inheritStr = $"{AL_GameObjectTool.GetInheritStr(mono.transform)} : {mono.GetType()} . {field.Name} => {selectGo.name} : {selectComponent.GetType()}";
                            CheckAddHierarchyReferenceMsg(mono.gameObject, inheritStr);
                        } else if (field.FieldType == typeof(GameObject) && ReferenceEquals(val, selectGo))
                        {
                            inheritStr = $"{AL_GameObjectTool.GetInheritStr(mono.transform)} : {mono.GetType()} . {field.Name} => {selectGo.name}";
                            CheckAddHierarchyReferenceMsg(mono.gameObject, inheritStr);
                        }
                    }
                }
            }
        }

        private void OnHierarchyPartUpdate()
        {
            if (Selection.activeObject != null && Selection.activeObject is GameObject selectGo)
            {
                if (_dicHierarchyPart == null)
                {
                    _OnSelectionChange();
                }
                scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                if (_dicHierarchyPart.Count > 0)
                {
                    EditorGUILayout.LabelField("当前选择在Hierarchy界面被如下引用：");
                }
                else
                {
                    EditorGUILayout.LabelField("当前选择在Hierarchy界面无引用：");
                }
                foreach (var kv in _dicHierarchyPart)
                {
                    EditorGUILayout.ObjectField( kv.Key, typeof(Object), false);
                    EditorGUILayout.LabelField($"对于{selectGo.name}有{kv.Value.Count}个引用，如下：");
                    foreach (var str in kv.Value)
                    {
                        var index = str.IndexOf("=>");
                        if (index != -1)
                        {
                            EditorGUILayout.LabelField(str.Substring(0, index));
                            EditorGUILayout.LabelField(str.Substring(index));
                            EditorGUILayout.Space();
                        }
                    }
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();
            }
        }
        
        protected override void OnUpdateUI()
        {
            OnTipPartUpdate();
            OnChoosePartUpdate();
            OnDependenciesPartUpdate();
            OnReferencesPartUpdate();
            OnHierarchyPartUpdate();
            OnOperationPartUpdate();
        }
    }
    
    public class AL_ReferenceHelper: AssetPostprocessor
    {
        public static string RootPath = Application.dataPath.Replace("Assets", "");

        public static bool IsDir(string assetPath)
        {
            return Directory.Exists(Path.Combine(RootPath, assetPath));
        }

        public static bool IsIllegal(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) || IsDir(assetPath);
        }

        //绝对路径转相对路径
        public static string Absolute2Relative(string absolutePath)
        {
            return absolutePath.Replace(RootPath, "");
        }
        //相对路径转绝对路径
        public static string Relative2Absolute(string relativePath)
        {
            return RootPath + relativePath;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var hasShow = EditorWindow.HasOpenInstances<AL_ReferenceHelperWindow>();
            if (hasShow)
            {
                var moduleData = AlchemyEditorModule.Instance.GetEditorModuleRuntimeData<ReferenceHelperData>(AL_ReferenceHelperWindow.ModuleSign);
                if (moduleData == null) return;
                foreach (var importedAsset in importedAssets)
                {
                    if (IsDir(importedAsset)) continue;
                    moduleData.OnImportAsset(importedAsset);
                }
                
                foreach (var deletedAsset in deletedAssets)
                {
                    if (IsDir(deletedAsset)) continue;
                    moduleData.OnDeletedAsset(deletedAsset);
                }
                moduleData.OnMovedAssets(movedAssets, movedFromAssetPaths);
            }
            
        }
    }
    
    [Serializable]
    public class ReferenceHelperData:AlchemyBaseEditorModuleData
    {
        public AL_SerializableDictionary<string,ReferenceRelationshipData> DicRelationship = new AL_SerializableDictionary<string, ReferenceRelationshipData>();
        
        public void ReInit(List<string> assetPaths)
        {
            DicRelationship.Clear();
            foreach (var assetPath in assetPaths)
            {
                AddNewOneRelationshipData(assetPath);
            }
        }
        
        public void OnImportAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            DicRelationship.TryGetValue(assetPath, out var data);
            if (data == null)
            {
                AddNewOneRelationshipData(assetPath);
            }
            else
            {
                if (data.CheckIfDependenciesChanged(out var msg) && msg != null)
                {
                    OnAssetRelationshipChanged(msg);
                }
            }
        }

        public void OnAssetRelationshipChanged(ReferenceAssetChangedMsg msg)
        {
            if (AL_ReferenceHelper.IsIllegal(msg?.AssetPath) || !DicRelationship.TryGetValue(msg.AssetPath, out var assetData) || assetData == null) return;
            foreach (var depenPath in msg.AddDependencies.DataList)
            {
                assetData.AddDependencies(depenPath);
                DicRelationship.TryGetValue(depenPath, out var depenData);
                depenData??= AddNewOneRelationshipData(depenPath);
                depenData?.AddReference(msg.AssetPath);
            }

            foreach (var cutPath in msg.CutDependencies.DataList)
            {
                assetData.CutDependencies(cutPath);
                DicRelationship.TryGetValue(cutPath, out var cutData);
                cutData?.CutReference(msg.AssetPath);
            }
        }

        public void OnDeletedAsset(string assetPath)
        {
            if (AL_ReferenceHelper.IsIllegal(assetPath) || !DicRelationship.TryGetValue(assetPath, out var assetData) || assetData == null) return;
            foreach (var depenPath in assetData.DirectlyDependencies.DataList)
            {
                DicRelationship.TryGetValue(depenPath, out var depenData);
                depenData?.CutReference(assetPath);
            }

            foreach (var refPath in assetData.DirectlyReferences.DataList)
            {
                DicRelationship.TryGetValue(refPath, out var refData);
                refData?.CutDependencies(assetPath);
            }
        }

        public void OnMovedAssets(string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                var oldPath = movedFromAssetPaths[i];
                var newPath = movedAssets[i];
                if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath) || 
                    AL_ReferenceHelper.IsDir(oldPath) || AL_ReferenceHelper.IsDir(newPath)) continue;
                if (DicRelationship.ContainsKey(newPath)) Debug.LogError("【注意】移动资源到已有的资源路径下，依赖信息将被覆盖; " + newPath);
                DicRelationship.Remove(oldPath, out var assetData);
                if (assetData != null)
                {
                    ChangeAssetPath(assetData, newPath);
                    DicRelationship.TryAdd(newPath, assetData);
                }
            }
        }

        private void ChangeAssetPath(ReferenceRelationshipData assetData, string newPath)
        {
            if (assetData == null || string.IsNullOrEmpty(newPath)) return;
            foreach (var depenPath in assetData.DirectlyDependencies.DataList)
            {
                DicRelationship.TryGetValue(depenPath, out var depenData);
                depenData?.CutReference(assetData.AssetPath);
                depenData?.AddReference(newPath);
            }

            foreach (var refPath in assetData.DirectlyReferences.DataList)
            {
                DicRelationship.TryGetValue(refPath, out var refData);
                refData?.CutDependencies(assetData.AssetPath);
                refData?.AddDependencies(newPath);
            }
            assetData.AssetPath = newPath;
        }
        
        private ReferenceRelationshipData AddNewOneRelationshipData(string assetPath)
        {
            if (AL_ReferenceHelper.IsIllegal(assetPath) || DicRelationship.ContainsKey(assetPath)) return null;
            var curObjData = new ReferenceRelationshipData(assetPath);
            DicRelationship.Add(assetPath, curObjData);
            foreach (var dependencePath in curObjData.DirectlyDependencies.DataList)
            {
                DicRelationship.TryGetValue(dependencePath, out var dependenceData);
                dependenceData??=AddNewOneRelationshipData(dependencePath);
                dependenceData?.AddReference(assetPath);
            }
            return curObjData;
        }
        
        private AL_SerializableHashSet<string> CountReferences(ReferenceRelationshipData objData)
        {
            var curAllReferencesPaths = new AL_SerializableHashSet<string>();
            var curCountedPaths = new AL_SerializableHashSet<string>();
            CountAllReferences(objData, curAllReferencesPaths, curCountedPaths);
            return curAllReferencesPaths;
        }

        //递归计算所有引用到该资源的路径（直接跟间接）
        private void CountAllReferences(ReferenceRelationshipData objData, AL_SerializableHashSet<string> curAllReferencesPaths, AL_SerializableHashSet<string> curCountedPaths)
        {
            if ((objData?.DirectlyReferences?.Count ?? 0) <= 0 ) return;
            if (curCountedPaths.Contains(objData.AssetPath)) return;
            curAllReferencesPaths.AddRange(objData.DirectlyReferences);
            curCountedPaths.Add(objData.AssetPath);
            foreach (var assetPath in objData.DirectlyReferences.DataList)
            {
                DicRelationship.TryGetValue(assetPath, out var data);
                CountAllReferences(data,curAllReferencesPaths,curCountedPaths);
            }
        }

        private void UpdateDependencies(ICollection<string> assetPaths)
        {
            if ((assetPaths?.Count ?? 0) <= 0) return;
            foreach (var assetPath in assetPaths)
            {
                DicRelationship.TryGetValue(assetPath, out var data);
                data?.UpdateSelfDependencies();
            }
        }
    }
    
    [Serializable]
    public class ReferenceRelationshipData
    {
        public string AssetPath;
        public string SysPath;
        /// <summary>
        /// 直接依赖
        /// </summary>
        public AL_SerializableHashSet<string> DirectlyDependencies = new AL_SerializableHashSet<string>();
        /// <summary>
        /// 直接被引用
        /// </summary>
        public AL_SerializableHashSet<string> DirectlyReferences = new AL_SerializableHashSet<string>();
        

        public ReferenceRelationshipData(string assetPath)
        {
            SetAssetPath(assetPath);
        }

        public void SetAssetPath(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogError("不存在以下路径：" + assetPath);
                return;
            }
            AssetPath = assetPath;
            UpdateSelfDependencies();
        }

        public void CutReference(string assetPath)
        {
            DirectlyReferences.Remove(assetPath);
        }

        public void AddReference(string assetPath)
        {
            DirectlyReferences.Add(assetPath);
        }

        public void AddDependencies(string assetPath)
        {
            DirectlyDependencies.Add(assetPath);
        }

        public void CutDependencies(string assetPath)
        {
            DirectlyDependencies.Remove(assetPath);
        }

        public void UpdateSelfDependencies()
        {
            DirectlyDependencies = new AL_SerializableHashSet<string>(AssetDatabase.GetDependencies(AssetPath, false));
        }

        public bool CheckIfDependenciesChanged(out ReferenceAssetChangedMsg msg)
        {
            var newDependencies = new AL_SerializableHashSet<string>(AssetDatabase.GetDependencies(AssetPath, false));
            msg = null;
            if (DirectlyDependencies.Equals(newDependencies))
            {
                return false;
            }
            msg = new ReferenceAssetChangedMsg(AssetPath, DirectlyDependencies, newDependencies);
            return true;
        }
    }
    
    public class ReferenceAssetChangedMsg
    {
        public string AssetPath;
        public AL_SerializableHashSet<string> AddDependencies = new AL_SerializableHashSet<string>();
        /// <summary>
        /// 注意这里只是清除掉依赖，而不是删除掉依赖文件
        /// </summary>
        public AL_SerializableHashSet<string> CutDependencies = new AL_SerializableHashSet<string>();

        public ReferenceAssetChangedMsg(string assetPath, AL_SerializableHashSet<string> oldOne, AL_SerializableHashSet<string> newOne)
        {
            AssetPath = assetPath;
            if (oldOne?.Equals(newOne) ?? newOne == null) return;//相等直接返回
            if (oldOne == null || oldOne.Count == 0)
            {
                AddDependencies = newOne;
                return;
            }

            if (newOne == null || newOne.Count == 0)
            {
                CutDependencies = oldOne;
                return;
            }
            
            CutDependencies = oldOne.ExceptWith(newOne);
            AddDependencies = newOne.ExceptWith(oldOne);
        }
    }
}