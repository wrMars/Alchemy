using System.IO;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_PathTools
    {
        private static string _rootPath;//项目根目录 含/号
        public static string RootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_rootPath))
                {
                    _rootPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
                }
                return _rootPath;
            }
        }
        
        public static string AssetPath2FullPath(string assetPath)
        {
            return Path.Combine(RootPath, assetPath);
        }
        
        public static string FullPath2AssetPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            return fullPath.Replace(RootPath, "");
        }

        public static string AssetPath2FileName(string assetPath, out string ex)
        {
            var filePath = AssetPath2FullPath(assetPath);
            return Path2FileName(filePath, out ex);
        }

        public static string Path2FileName(string filePath, out string ex)
        {
            var fileName = Path.GetFileName(filePath);
            var arr =  fileName.Split('.');
            ex = "";
            if (arr.Length == 2)
            {
                ex = arr[1];
                return arr[0];
            }
            return "";
        }

        public static bool ExistsAssetFile(string assetPath)
        {
            return File.Exists(AssetPath2FullPath(assetPath));
        }

        public static string Combine(string a, string b)
        {
            var re = Path.Combine(a, b);
            return re.Replace("\\", "/");
        }
    }
}