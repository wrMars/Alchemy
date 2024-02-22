using System;
using System.Collections;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class Al_Main : MonoBehaviour
    {
        private static bool _init;
        [RuntimeInitializeOnLoadMethod]
        public static void AlchemyMainInit()
        {
            if (_init) return;
            _init = true;
            var go = new GameObject("Al_Main");
            go.AddComponent<Al_Main>();
            go.AddComponent<AL_TimeMgr>();
            DontDestroyOnLoad(go);
        }
        
        public static Al_Main instance;
        private void Awake()
        {
            instance = this;
        }

        private void OnApplicationQuit()
        {
            UnityEngine.Debug.LogError("游戏退出触发");
            AL_EventMgr.Action(new AlGamePreQuit());
        }

        public void TryStopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        
        /// <summary>
        /// 解决TimeScale对协程WaitForSeconds的影响
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="waitTime">等待时间 秒</param>
        /// <param name="ignoreTimeScale">是否忽略TimeScale影响</param>
        public Coroutine StartCoroutine(Action callback, float waitTime, bool ignoreTimeScale = false)
        {
            return StartCoroutine(IEnumeratorForTime(callback, waitTime, ignoreTimeScale));
        }
        IEnumerator IEnumeratorForTime(Action callback, float waitTime, bool ignoreTimeScale = false)
        {
            if (ignoreTimeScale)
            {
                float start = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup < start + waitTime)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }
            callback();
        }

        public Coroutine HelpToStartCoroutine(IEnumerator fun)
        {
            return StartCoroutine(fun);
        }
    }
}