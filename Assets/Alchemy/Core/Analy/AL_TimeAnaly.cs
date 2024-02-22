using System.Collections.Generic;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public static class AL_TimeAnaly
    {
        private static Dictionary<string, float> _noteTime = new Dictionary<string, float>();
        private static Dictionary<string, float> _startTime = new Dictionary<string, float>();
        private static readonly bool _allowLog = false;

        public static void Start(string logo)
        {
            if (_allowLog)
            {
                _noteTime[logo] = Time.realtimeSinceStartup;
                _startTime[logo] = Time.realtimeSinceStartup;
            }
        }

        public static void Note(string logo, string stepLogo)
        {
            if (_allowLog)
            {
                var pastNote = _noteTime[logo];
                UnityEngine.Debug.LogError($"[TimeNote]==【{logo}】===>({stepLogo}): {Time.realtimeSinceStartup - pastNote}");
                _noteTime[logo] = Time.realtimeSinceStartup;
            }
        }

        public static void End(string logo)
        {
            if (_allowLog)
            {
                UnityEngine.Debug.LogError($"[TimeNote]==【{logo}】===>总耗时: {Time.realtimeSinceStartup - _startTime[logo]}");
                _noteTime[logo] = 0;
                _startTime[logo] = 0;
            }
        }

        public static void TryLog(string logo, float pastNoteTime)
        {
            if (_allowLog)
            {
                UnityEngine.Debug.LogError($"[TimeNote]==【{logo}】===>耗时: {Time.realtimeSinceStartup - pastNoteTime}");
            }
        }
        
        public static void TryLogSpc(string logo, float pastNoteTime, int id)
        {
            if (_allowLog && id == 88)
            {
                UnityEngine.Debug.LogError($"[TimeNote]==【{logo}】===>耗时: {Time.realtimeSinceStartup - pastNoteTime}");
            }
        }
    }
}