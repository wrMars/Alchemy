/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Alchemy
{
    public class AL_HierarchyToolWindow : EditorWindow
    {
        [MenuItem("Alchemy/界面拓展/Hierarchy拓展")]
        private static void ShowWindow()
        {
            var window = GetWindow<AL_HierarchyToolWindow>();
            window.titleContent = new GUIContent("Hierarchy拓展");
            window.Show();
        }

        private void OnGUI()
        {
            AL_HierarchyViewTool.OpenShowID = EditorGUILayout.Toggle("选中显示InstanceID", AL_HierarchyViewTool.OpenShowID);
            AL_HierarchyViewTool.OpenAllShowID = EditorGUILayout.Toggle("显示所有InstanceID", AL_HierarchyViewTool.OpenAllShowID);
            AL_HierarchyViewTool.OpenUIChooseHelper = EditorGUILayout.Toggle("界面选中辅助(game右键)", AL_HierarchyViewTool.OpenUIChooseHelper);
            AL_MouseTouchHelper.TrySwitch(AL_HierarchyViewTool.OpenUIChooseHelper);
        }
    }
    
    [InitializeOnLoad]
    public class AL_HierarchyViewTool
    {
        public static bool OpenShowID;
        public static bool OpenAllShowID;
        public static bool OpenUIChooseHelper;
        static AL_HierarchyViewTool()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
        }
        
        private static void HierarchyItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (!OpenAllShowID && (!OpenShowID || instanceID != Selection.activeInstanceID)) return;
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null)
            {
                Rect idRect = new Rect(selectionRect);
                idRect.x = selectionRect.xMax - 80f;
                idRect.width = 80f;
                EditorGUI.LabelField(idRect, instanceID.ToString());
            }
        }
    }
    
    public class AL_MouseTouchHelper : MonoBehaviour
    {
        private static AL_MouseTouchHelper CurComponent;
        public static void TrySwitch(bool open)
        {
            if (Al_Main.instance == null) return;
            CurComponent ??= Al_Main.instance.gameObject.GetComponent<AL_MouseTouchHelper>();
            if (open && CurComponent == null)
            {
                Al_Main.instance.gameObject.AddComponent<AL_MouseTouchHelper>();
            } else if (!open && CurComponent != null)
            {
                DestroyImmediate(CurComponent);
                CurComponent = null;
            }
        }

        private void OnDestroy()
        {
            CurComponent = null;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, results);
                if (results.Count > 0)
                {
                    if (results[0].gameObject != null)
                    {
                        EditorGUIUtility.PingObject(results[0].gameObject);
                        Selection.SetActiveObjectWithContext(results[0].gameObject, null);
                    }
                    for (int i = 0; i < results.Count; i++)
                    {
                        GameObject hitObject = results[i].gameObject;
                        if (hitObject != null) UnityEngine.Debug.LogWarning("Mouse is hovering over: " + hitObject.name);
                    }
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject != null )
                    {
                        GameObject hitObject = hit.collider.gameObject;
                        EditorGUIUtility.PingObject(hitObject);
                        Selection.SetActiveObjectWithContext(hitObject, null);
                        UnityEngine.Debug.LogWarning("Mouse is hovering over: " + hitObject.name);
                    }
                }
            }
        }
    }
}