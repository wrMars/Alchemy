/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using UnityEditor;

namespace Alchemy
{
    public class AL_EditorWindowTool
    {
        public static void GuiHorizontal(Action func)
        {
            EditorGUILayout.BeginHorizontal();
            func?.Invoke();
            EditorGUILayout.EndHorizontal();
        }
    }
}