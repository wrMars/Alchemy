using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 短命字典，空闲一段时间自动销毁
    /// </summary>
    public class AL_BriefDictionary<TKEY, TVAL>
    {
        private float _freeDelTime;
        private Dictionary<TKEY, TVAL> _tempDic;
        private Coroutine curCoroutine;
        
        public AL_BriefDictionary(float freeDelTime)
        {
            _freeDelTime = freeDelTime;
        }

        public bool TryGetValue(TKEY key, out TVAL val)
        {
            if (_tempDic != null)
            {
                Action();
                return _tempDic.TryGetValue(key, out val);
            }
            else
            {
                val = default(TVAL);
                return false;
            }
        }

        public void SetValue(TKEY key, TVAL val)
        {
            if (_tempDic == null) CreateNewDic();
            _tempDic[key] = val;
            Action();
        }

        private void Action()
        {
            UpdateCountdown();
        }

        private void CreateNewDic()
        {
            Destroy();
            _tempDic = new Dictionary<TKEY, TVAL>();
            StartCountdownToDispose();
        }

        private void UpdateCountdown()
        {
            DisposeTime = Time.realtimeSinceStartup + _freeDelTime;
        }

        private float DisposeTime = 0;
        IEnumerator LoopForDispose()
        {
            while (Time.realtimeSinceStartup < DisposeTime)
            {
                yield return null;
            }
            Destroy();
        }

        private void StartCountdownToDispose()
        {
            UnityEngine.Debug.Log("【AL_BriefDictionary】开始计时，空闲一段时间自动销毁");
            DisposeTime = Time.realtimeSinceStartup + _freeDelTime;
            Al_Main.instance.TryStopCoroutine(curCoroutine);
            curCoroutine = Al_Main.instance.HelpToStartCoroutine(LoopForDispose());
        }

        public void Destroy(bool force = false)
        {
            UnityEngine.Debug.Log("【AL_BriefDictionary】销毁");
            _tempDic?.Clear();
            _tempDic = null;
            if (force)
            {
                Al_Main.instance.TryStopCoroutine(curCoroutine);
                curCoroutine = null;
            }
        }
    }
}