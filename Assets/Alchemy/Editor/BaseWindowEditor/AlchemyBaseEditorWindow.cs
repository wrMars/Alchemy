using UnityEditor;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public abstract class AlchemyBaseEditorWindow<TWindow,TData> : EditorWindow where TWindow :EditorWindow, new() where TData :AlchemyBaseEditorModuleData, new()
    {
        public static EditorWindow CurWindow;
        public abstract string ModuleName { get; set; }
        protected TData ModuleData;

        private bool _inited;
        private void OnEnable()
        {
            if (!_inited)
            {
                // CurWindow = this;
                ModuleData = AlchemyEditorModule.Instance.GetEditorModuleRuntimeData<TData>(ModuleName);
                ModuleData.Window = CurWindow;
                StaticEventReAdd();
                _inited = true;
            }
        }

        public static void OpenWindow(string title)
        {
            CurWindow = GetWindow<TWindow>(title);
            CurWindow.Show();
        }

        private void OnDestroy()
        {
            StaticEventReset();
        }

        private void OnGUI()
        {
            if (CurWindow == null)
            {
                CurWindow = ModuleData.Window;
                StaticEventReAdd();
            }
            OnUpdateUI();
        }

        private float _passedTime;
        private float _targetTime = 0.5f;
        private void Update()
        {
            if(_passedTime>_targetTime)
            {
                _passedTime = 0;
                Repaint();
            }
            _passedTime += Time.deltaTime;
        }

        protected abstract void StaticEventReAdd();
        protected abstract void StaticEventReset();

        protected abstract void OnUpdateUI();
    }
}