/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using UnityEditor;
using UnityEngine;

namespace Alchemy
{
    public class AL_GameObjectCompareWindow : EditorWindow
    {
        [MenuItem("Alchemy/prefab对比工具")]
        private static void ShowWindow()
        {
            var window = GetWindow<AL_GameObjectCompareWindow>();
            window.titleContent = new GUIContent("prefab对比工具");
            window.Show();
        }

        private Object _obj1;
        private Object _obj2;
        private AL_GameObjectDiffInfo _diffInfo1;
        private AL_GameObjectDiffInfo _diffInfo2;
        
        private void OnGUI()
        {
            _obj1 = EditorGUILayout.ObjectField("A prefab", _obj1, typeof(Object), false);
            _obj2 = EditorGUILayout.ObjectField("B prefab",_obj2, typeof(Object), false);
            if (GUILayout.Button("对比"))
            {
                if (_obj1 is GameObject go1 && PrefabUtility.IsPartOfPrefabAsset(go1) && _obj2 is GameObject go2 && PrefabUtility.IsPartOfPrefabAsset(go2))
                {
                    var re = AL_GameObjectTool.IsObjEquals(go1, go2, out _diffInfo1, out _diffInfo2);
                    UnityEngine.Debug.LogError(re);
                    UnityEngine.Debug.LogError(_diffInfo1?.ToString());
                    UnityEngine.Debug.LogError(_diffInfo2?.ToString());
                }
                else
                {
                    AL_LogTool.LogOnlyEditor("类型不对, 不是prefab");
                }
            }
            AL_EditorWindowTool.GuiHorizontal(() =>
            {
                if (GUILayout.Button("输出A->B差异") && _diffInfo1 != null && _diffInfo2 != null)
                {
                    _diffInfo1.SaveInfo();
                }
                if (GUILayout.Button("输出B->A差异") && _diffInfo1 != null && _diffInfo2 != null)
                {
                    _diffInfo2.SaveInfo();
                }
            });
        }
        
    }
}