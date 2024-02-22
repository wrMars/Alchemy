/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alchemy
{
    public class AL_IOTool
    {
        public static void WriteInFile(string filePath, string msg)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(filePath, msg, Encoding.UTF8);
        }
        
        public static void AppendWriteInFile(string filePath, string msg)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
                fs.Position = fs.Length;
                fs.Write(data, 0, data.Length);
                fs.Flush();
            }
        }
        
        public static void ReadFileByLines(string filePath, Func<string, bool> func)
        {
            if (func == null) return;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string line;
                    while (!reader.EndOfStream)
                    {
                        func.Invoke(reader.ReadLine());
                    }
                }
            }
        }
    }
}