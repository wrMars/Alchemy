
/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Alchemy
{
    public static class AL_ScriptableObjectTools
    {
        static List<ScriptableObject> list = new List<ScriptableObject>();
        [MenuItem("Assets/Alchemy/Create ScriptableObject Asset", priority = 1)]
        static void Create()
        {
            list.Clear();
            foreach (var item in Selection.objects)
            {
                list.Add(CreateAsset(item));
            }
            Selection.objects = list.ToArray();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
        }
        
        [MenuItem("Assets/Alchemy/Create ScriptableObject Asset", true)]
        static bool Validate()
        {
            Func<Object, bool> predicate = (obj) =>
            {
                if (obj is MonoScript)
                {
                    var ms = obj as MonoScript;
                    var type = ms.GetClass();
                    if (null != type)
                    {
                        var valid = type.IsSubclassOf(typeof(ScriptableObject));
                        // Debug.Assert(valid, $"创建失败：选择了错误的类型 → {type}");
                        return !valid;
                    }
                    // Debug.LogError($"创建失败: 请避免选择静态类型和没写继承关系的patial类型 → {ms.name}");
                }
                return true;
            };
            return Selection.objects.Length > 0 && !Selection.objects.Any(predicate);
        }
        
        static ScriptableObject CreateAsset(Object ms)
        {
            var path = AssetDatabase.GetAssetPath(ms);
            path = path.Substring(0, path.LastIndexOf("/"));
            // path = Path.Combine(path,"Data");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var type = (ms as MonoScript).GetClass();
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/ {type.Name}.asset");
            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
#endif