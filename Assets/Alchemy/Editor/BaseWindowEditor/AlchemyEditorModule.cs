using System.Collections.Generic;
using UnityEditor;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    [InitializeOnLoad]
    public class AlchemyEditorModule
    {
        static AlchemyEditorModule()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnQuit;
        }

        private static void OnQuit()
        {
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode://停止播放事件监听后被监听
                    // Instance.SaveEditorDataImmediately(true);
                    // Debug.LogError("如果编辑器应用程序处于编辑模式而之前处于播放模式，则在编辑器应用程序的下一次更新期间发生。");
                    break;
                case PlayModeStateChange.ExitingEditMode://编辑转播放时监听(播放之前)
                    // Instance.SaveEditorDataImmediately(true);
                    // Debug.LogError("在退出编辑模式时，在编辑器处于播放模式之前发生。");
                    break;
                case PlayModeStateChange.EnteredPlayMode://播放时立即监听
                    // Debug.LogError("如果编辑器应用程序处于播放模式而之前处于编辑模式，则在编辑器应用程序的下一次更新期间发生。");
                    break;
                case PlayModeStateChange.ExitingPlayMode://停止播放立即监听
                    // Debug.LogError("在退出播放模式时，在编辑器处于编辑模式之前发生。");
                    break;
            }
        }

        private static AlchemyEditorModule _instance;
        public static AlchemyEditorModule Instance
        {
            get
            {
                _instance ??= new AlchemyEditorModule();
                return _instance;
            }
        }

        private Dictionary<string, AlchemyBaseEditorModuleData> EditorModuleRuntimeDatas = new Dictionary<string, AlchemyBaseEditorModuleData>();

        public T GetEditorModuleRuntimeData<T>(string moduleName) where T:AlchemyBaseEditorModuleData, new()
        {
            if (EditorModuleRuntimeDatas.TryGetValue(moduleName, out var data))
            {
                return data as T;
            }
            else
            {
                var re = new T();
                EditorModuleRuntimeDatas.Add(moduleName, re);
                return re;
            }
        }
    }
}