/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections;
using System.Reflection;

namespace Alchemy
{
    public class AL_CfgLoader
    {
        public static string GetAssetPath(string assetFileName)
        {
            return string.Format( AL_CfgParser.ScriptableAssetPath, assetFileName);
        }

        public static bool PreLoadComplete { get; private set; } = false;
        public static void PreLoadAllDatas(Action callback)
        {
            if (PreLoadComplete)
            {
                callback?.Invoke();
                return;
            }
            var list = AL_CfgDataAttribute.GetAllUserInAssembly();
            int curNum = 0;
            foreach (var t in list)
            {
                MethodInfo methodInfo = t.GetMethod("LoadAssets", BindingFlags.Public | BindingFlags.Static);
                if (methodInfo != null)
                {
                    Action<Func<AL_BaseCfgVos, bool>> action = (Func<AL_BaseCfgVos, bool> func) =>
                    {
                        methodInfo.Invoke(null, new object[] { func });
                    };
                    action.Invoke((AL_BaseCfgVos data) =>
                    {
                        curNum++;
                        if (curNum == list.Count - 1) {
                            PreLoadComplete = true;//要排除AL_BaseCfgVos的情况
                            callback?.Invoke();
                        }
                        return true;
                    });
                }
            }
        }

        public static void PreLoadAllDatasCoroutine()
        {
            Al_Main.instance.HelpToStartCoroutine(PreLoadAllDatas());
        }

        public static IEnumerator PreLoadAllDatas()
        {
            if (!PreLoadComplete)
            {
                PreLoadAllDatas(null);
                while (false == PreLoadComplete)
                    yield return null;
            }
        }
    }
}