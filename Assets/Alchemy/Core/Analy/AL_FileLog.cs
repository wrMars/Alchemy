/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Alchemy
{
    /// <summary>
    /// 输出日志到文件
    /// Windows: %userprofile%\AppData\LocalLow\<companyname>\<productname>.
    /// WebGL: /idbfs/<md5 hash of data path> where the data path is the URL stripped of everything including and after the last '/' before any '?' components.
    /// iOS:  /var/mobile/Containers/Data/Application/<guid>/Documents.
    /// Android:  /storage/emulated/0/Android/data/<packagename>/files 
    /// </summary>
    public class AL_FileLog
    {
        private static string _logDirPath = Path.Combine(Application.persistentDataPath, "AL_log");
        private const string Logo = "[AL_FileLog]";
        private static bool _inited = false;

        [RuntimeInitializeOnLoadMethod]
        private static void CheckInitListener()
        {
            if (_inited) return;
            _inited = true;
            Application.logMessageReceived += HandleLogMessage;
        }
        
        private static void HandleLogMessage(string logString, string stackTrace, LogType type)
        {
            // if (Application.isEditor) return;
            if (type == LogType.Exception || (type == LogType.Error && logString.Contains(Logo)))
            {
                if (type == LogType.Exception)
                {
                    LogToFile("\n" + logString + "\n");
                    LogToFile(stackTrace + "\n");
                    UnityEngine.Debug.Log($"全局异常捕捉msg：{logString}   stack：{stackTrace}");
                }
                else
                {
                    LogToFile(logString);
                }
            }
        }
        
        public static void LogError(string msg)
        {
            CheckInitListener();
            _curInfo = $"{Logo}{GetCurLogTimeString()} {msg}\n";
            UnityEngine.Debug.LogError(_curInfo);
        }

        private static Dictionary<string, bool> _waitLogMsgs = new Dictionary<string, bool>();
        /// <summary>
        /// 延迟写入本地日志，一般用于不重要/频繁的打印（会筛选掉重复的打印）
        /// </summary>
        /// <param name="msg"></param>
        public static void LogErrorLater(string msg)
        {
            CheckAddLoop();
            if (!_waitLogMsgs.ContainsKey(msg))
            {
                _waitLogMsgs.Add(msg, true);
            }
        }

        private static bool HadAddLoopLog;
        private static void CheckAddLoop()
        {
            if (HadAddLoopLog) return;
            HadAddLoopLog = true;
            Al_Main.instance.HelpToStartCoroutine(LoopCheckLog());
        }

        private static StringBuilder _stringBuilder = new StringBuilder();
        private static IEnumerator LoopCheckLog()
        {
            while (true)
            {
                yield return null;
                if (_waitLogMsgs.Count == 0) continue;
                _stringBuilder.Clear();
                foreach (var kv in _waitLogMsgs)
                {
                    _stringBuilder.Append($"{Logo}{GetCurLogTimeString()} {kv.Key}\n");
                }

                if (_stringBuilder.Length > 0)
                {
                    UnityEngine.Debug.LogError(_stringBuilder.ToString());
                    _waitLogMsgs.Clear();
                }
            }
        }
        
        private static string GetDayTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        private static string GetCurLogTimeString()
        {
            return DateTime.Now.ToString("[HH:mm:ss]");
        }
        
        private static string GetCurFilePath()
        {
            return Path.Combine(_logDirPath, $"{GetDayTimeString()}.txt");
        }

        private static string _curInfo;
        private static string _curFilePath;
        private static void LogToFile(string msg)
        {
            _curFilePath = GetCurFilePath();
            CheckFirst();
            AL_IOTool.AppendWriteInFile(_curFilePath, msg);
        }

        private static bool _checkedFile = false;
        private static int _retainLogFileNum = 5;
        private static void CheckFirst()
        {
            var curDay = GetDayTimeString();
            if (!_checkedFile && Directory.Exists(_logDirPath))
            {
                List<string> files = Directory.GetFiles(_logDirPath, "*.txt").ToList();
                int needDelNum = files.Count - _retainLogFileNum;
                if (needDelNum > 0)
                {
                    files.Sort();
                    for (int i = 0; i < needDelNum; i++)
                    {
                        if (!files[i].Contains(curDay)) File.Delete(files[i]);
                    }
                }
            }
            _checkedFile = true;
        }

#if UNITY_EDITOR

        [MenuItem("Alchemy/打开AL_log文件夹")]
        private static void OpenLogPath()
        {
            System.Diagnostics.Process.Start(_logDirPath);
        }
#endif
    }
}