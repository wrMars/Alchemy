using System;
using UnityEditor;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy.Core.Encrypt.Editor
{
    public class AL_NumEncryptWindow:EditorWindow
    {
        private string curNumStr = "";
        private string encryptStr = "";
        private string xorStr = "";
        
        [MenuItem("Alchemy/数字加解密测试工具")]
        private static void ShowWindow()
        {
            GetWindow<AL_NumEncryptWindow>("数字加解密工具").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            curNumStr = EditorGUILayout.TextField("输入原始数据：", curNumStr);
            if (GUILayout.Button("加密"))
            {
                var time = 1000;
                var cur = DateTime.Now.Ticks;
                UnityEngine.Debug.LogError($"数据：{curNumStr}; 加密的数据为：{AL_NumEncrypt.NumEncrypt.Encrypt(int.Parse(curNumStr))}");
                for (int i = 0; i < time; i++)
                {
                    AL_NumEncrypt.Encrypt(int.Parse(curNumStr));
                }
                UnityEngine.Debug.LogError($"{time}次耗时：{(DateTime.Now.Ticks - cur)/10000}毫秒");
            }
            EditorGUILayout.EndHorizontal();
                
            EditorGUILayout.BeginHorizontal();
            encryptStr = EditorGUILayout.TextField("输入加密数据：", encryptStr);
            if (GUILayout.Button("解密"))
            {
                var cur = DateTime.Now.Ticks;
                bool result = false;
                int outNum = 0;
                var time = 1000;
                for (int i = 0; i < time; i++)
                {
                    result = AL_NumEncrypt.TryDecrypt(encryptStr, out outNum);
                }
                if (result)
                {
                    UnityEngine.Debug.LogError($"数据：{encryptStr}; 解密后数据为：{outNum.ToString()}");
                }
                else
                {
                    UnityEngine.Debug.LogError("解密失败，数据异常【现在策略是，每次游戏运行时候都重新生成密码本；有可能是运行过、重开过unity导致密码本重新生成导致的解析失败】");
                }
                UnityEngine.Debug.LogError($"{time}次耗时：{(DateTime.Now.Ticks - cur)/10000}毫秒");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            xorStr = EditorGUILayout.TextField("输入异或数据：", xorStr);
            if (GUILayout.Button("异或"))
            {
                UnityEngine.Debug.LogError(AL_NumEncrypt.NumEncrypt.GetYhNum(int.Parse(xorStr)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("更新密码本"))
            {
                AL_NumEncrypt.UpdateNumEncryptor();
            }
        }
    }
}