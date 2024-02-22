/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using UnityEngine;

namespace Alchemy
{
    public class AL_LogTool
    {
        public static void LogOnlyEditor(string msg)
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.LogError(msg);
            }
        }
    }
}