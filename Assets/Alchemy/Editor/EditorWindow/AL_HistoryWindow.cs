using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_HistoryConst
    {
        public static readonly string[] TabNames = {"预设", "场景", "选中过"};
        public const int MaxNoteNum = 20;
    }
    
    public class AL_HistoryWindow:AlchemyBaseEditorWindow<AL_HistoryWindow,HistoryWindowData>
    {
        public override string ModuleName { get; set; } = "HistoryWindow";

        [MenuItem("Alchemy/历史编辑记录工具")]
        private static void ShowWindow()
        {
            OpenWindow("编辑历史记录");
        }

        //运行后监听、普通数据失效，要重新添加
        protected override void StaticEventReAdd()
        {
            StaticEventReset();
            EditorSceneManager.sceneOpened += NoteOpenScene;
            PrefabStage.prefabStageOpened += NoteOpenPrefab;
            EditorApplication.projectWindowItemOnGUI += NoteTouch;
        }

        protected override void StaticEventReset()
        {
            EditorSceneManager.sceneOpened -= NoteOpenScene;
            PrefabStage.prefabStageOpened -= NoteOpenPrefab;
            EditorApplication.projectWindowItemOnGUI -= NoteTouch;
        }

        Vector2 scrollPos = Vector2.zero;
        //tabIndex   "预设", "场景", "选中过"
        private void NoteOpenScene(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            ModuleData.NoteEditedPath(1, scene.path);
        }

        private void NoteOpenPrefab(PrefabStage stage)
        {
            ModuleData.NoteEditedPath(0, stage.assetPath);
        }

        void NoteTouch(string guid, Rect rect)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var activePath = AssetDatabase.GUIDToAssetPath(guid);
            if (activePath == path)
            {
                ModuleData.NoteEditedPath(2, path);
            }
        }

        protected override void OnUpdateUI()
        {
            ModuleData.CurTabIndex = GUILayout.Toolbar(ModuleData.CurTabIndex, AL_HistoryConst.TabNames);
            UpdatePegPart();
            UpdateNotePart();
        }

        void UpdatePegPart()
        {
            int delIndex = -1;
            var subData = ModuleData.GetSubWindowData(ModuleData.CurTabIndex);
            var noteNum = subData?.pegPaths.Count;
            if (noteNum > 0)
            {
                EditorGUILayout.LabelField(string.Format("固定：(最大记录数量{0})", AL_HistoryConst.MaxNoteNum));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                for (int i = 0; i < subData.pegPaths.Count; i++)
                {
                    var path = subData.pegPaths[i];
                    EditorGUILayout.BeginHorizontal();
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    EditorGUILayout.ObjectField(obj, typeof(Object), false);

                    if (GUILayout.Button("清除"))
                    {
                        delIndex = i;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (delIndex > -1)
                {
                    ModuleData.DelPegPath(ModuleData.CurTabIndex, delIndex);
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.Space();
            }
        }

        void UpdateNotePart()
        {
            int delIndex = -1;
            var subData = ModuleData.GetSubWindowData(ModuleData.CurTabIndex);
            var noteNum = subData?.editedPaths.Count;
            if (noteNum > 0)
            {
                EditorGUILayout.LabelField(string.Format("编辑历史：(最大记录数量{0})", AL_HistoryConst.MaxNoteNum));
                subData.scrollPos = EditorGUILayout.BeginScrollView(subData.scrollPos);
                for (int i = 0; i < subData.editedPaths.Count; i++)
                {
                    var path = subData.editedPaths[i];
                    EditorGUILayout.BeginHorizontal();
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    EditorGUILayout.ObjectField(obj, typeof(Object), false);
                    if (GUILayout.Button("固定"))
                    {
                        ModuleData.NotePegPath(ModuleData.CurTabIndex, path);
                    }
                    if (GUILayout.Button("清除"))
                    {
                        delIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (delIndex > -1)
                {
                    ModuleData.DelEditedPath(ModuleData.CurTabIndex, delIndex);
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("清除编辑历史", GUILayout.Height(30)))
                {
                    ModuleData.ClearEditeds(ModuleData.CurTabIndex);
                }
                if (GUILayout.Button("清除固定记录", GUILayout.Height(30)))
                {
                    ModuleData.ClearPegs(ModuleData.CurTabIndex);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
    
    [Serializable]
    public class HistorySubWindowData
    {
        public int tabIndex = 0;
        public Vector2 scrollPos = Vector2.zero;
        public List<string> editedPaths = new List<string>();
        public List<string> pegPaths = new List<string>();

        public HistorySubWindowData(int i)
        {
            tabIndex = i;
        }

        private void AddTo(List<string> arr, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (arr.Contains(path))
            {
                arr.Remove(path);
            }
            arr.Insert(0, path);
            if (arr.Count > AL_HistoryConst.MaxNoteNum)
            {
                arr.RemoveRange(AL_HistoryConst.MaxNoteNum, arr.Count - AL_HistoryConst.MaxNoteNum);
            }
        }

        public void AddEdited(string path)
        {
            AddTo(editedPaths, path);
        }

        public void DelEditedAt(int i)
        {
            editedPaths.RemoveAt(i);
        }

        public void ClearEditeds()
        {
            editedPaths.Clear();
        }

        public void AddPeg(string path)
        {
            AddTo(pegPaths, path);
        }

        public void DelPegAt(int i)
        {
            pegPaths.RemoveAt(i);
        }

        public void ClearPegs()
        {
            pegPaths.Clear();
        }
    }
    
    [Serializable]
    public class HistoryWindowData:AlchemyBaseEditorModuleData
    {
        public List<HistorySubWindowData> NoteDatas = new List<HistorySubWindowData>();
        public int CurTabIndex = 0;
        
        // public HistoryWindow window;

        public HistoryWindowData()
        {
            for (int i = 0; i < 3; i++)
            {
                NoteDatas.Add(new HistorySubWindowData(i));
            }
        }

        public HistorySubWindowData GetSubWindowData(int tabIndex)
        {
            if (tabIndex >= NoteDatas.Count) return null;
            return NoteDatas[tabIndex];
        }
        
        public void NoteEditedPath(int tabIndex, string path)
        {
            GetSubWindowData(tabIndex)?.AddEdited(path);
        }

        public void DelEditedPath(int tabIndex, int index)
        {
            GetSubWindowData(tabIndex)?.DelEditedAt(index);
        }

        public void ClearEditeds(int tabIndex)
        {
            GetSubWindowData(tabIndex)?.ClearEditeds();
        }

        public void NotePegPath(int tabIndex, string path)
        {
            GetSubWindowData(tabIndex)?.AddPeg(path);
        }

        public void DelPegPath(int tabIndex, int index)
        {
            GetSubWindowData(tabIndex)?.DelPegAt(index);
        }

        public void ClearPegs(int tabIndex)
        {
            GetSubWindowData(tabIndex)?.ClearPegs();
        }
    }
}