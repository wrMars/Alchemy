using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public static class AL_EncodingTools
    {
        // private static Encoding getEncoding( string filePath)
        // {
        //
        //     using (var reader = new StreamReader(filePath, Encoding.Default, true))
        //     {
        //         if (reader.Peek() >= 0) // you need this!
        //             reader.Read();
        //
        //         Encoding encoding = reader.CurrentEncoding;
        //         reader.Close();
        //         return encoding;
        //     }
        // }
        //
        // private static void changeFileEncodingToUTF8BOM(string filePath)
        // {
        //     //用到了上面的函数getEncoding()
        //     Encoding oldEndcoding = getEncoding(filePath);
        //     UnityEngine.Debug.LogError($"原格式为：{oldEndcoding}");
        //
        //     //读取文件内容
        //     string str = string.Empty;
        //     using (StreamReader sr = new StreamReader(filePath, oldEndcoding))
        //     {
        //         str = sr.ReadToEnd();
        //         sr.Close();
        //     }
        //     //以UTF-8带BOM格式重新写入文件
        //     Encoding newEncoding = new UTF8Encoding(true);
        //     using (StreamWriter sw = new StreamWriter(filePath, false, newEncoding ))
        //     {
        //         sw.Write(str);
        //         sw.Close();
        //     }        
        // }

        [MenuItem("Assets/韩文格式转成UTF-8")]
        public static void Kr2UTF8()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path = AL_PathTools.AssetPath2FullPath(path);
            Kr2UTF8(path);   
        }
        
        private static void Kr2UTF8(string filePath)
        {
            Encoding oldEndcoding = Encoding.GetEncoding("EUC-KR");
 
            //读取文件内容
            string str = string.Empty;
            using (StreamReader sr = new StreamReader(filePath, oldEndcoding))
            {
                str = sr.ReadToEnd();
                sr.Close();
            }
            //以UTF-8带BOM格式重新写入文件
            Encoding newEncoding = new UTF8Encoding(true);
            using (StreamWriter sw = new StreamWriter(filePath, false, newEncoding ))
            {
                sw.Write(str);
                sw.Close();
            }      
        }

        [MenuItem("DN/韩文格式转成UTF-8[批量]")]
        public static void BatchKr2UTF8()
        {
            var logo = ".cs";
            var csList = AssetDatabase.GetAllAssetPaths().ToList().FindAll((path) => path.IndexOf("Assets/") == 0 && path.Substring(path.Length - logo.Length) == logo);
            foreach (var assetPath in csList)
            {
                var path = AL_PathTools.AssetPath2FullPath(assetPath);
                Kr2UTF8(path);
            }
        }
        
        // [MenuItem("Assets/韩文转中文")]
        // public static void Kr2Chinese()
        // {
        //     string koreanText = "안녕하세요"; // 韩文字符串
        //     TranslateKoreanToChinese(koreanText);
        //     return;
        //     Encoding koreanEncoding = Encoding.GetEncoding("ks_c_5601-1987");
        //     byte[] koreanBytes = koreanEncoding.GetBytes(koreanText);
        //
        //     Encoding chineseEncoding = Encoding.GetEncoding("GB2312");
        //     string chineseText = chineseEncoding.GetString(koreanBytes);
        //
        //     UnityEngine.Debug.LogError(chineseText);
        // }
        //
        // static string TranslateKoreanToChinese(string text)
        // {
        //     string apiUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=ko&tl=zh-CN&dt=t&q=";
        //
        //     string urlEncodedText = Uri.EscapeDataString(text);
        //     string requestUrl = apiUrl + urlEncodedText;
        //
        //     using (WebClient webClient = new WebClient())
        //     {
        //         webClient.Encoding = Encoding.UTF8;
        //         string response = webClient.DownloadString(requestUrl);
        //         UnityEngine.Debug.LogError(response);
        //         // 解析翻译结果
        //         string translatedText = response.Substring(4, response.IndexOf("\\", 4, StringComparison.Ordinal) - 4);
        //         UnityEngine.Debug.LogError(translatedText);
        //         return translatedText;
        //     }
        // }
    }
}