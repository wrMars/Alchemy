using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public abstract class AL_NumEncryptBase
    {
        //只在读取数据长度时候使用
        protected List<char> _encryptLenMap;
        //26条 0~9的映射
        protected List<char>[] _numMapGroup;
        protected byte[] _xorKey;
        protected int _numXorKey;

        public AL_NumEncryptBase()
        {
            _encryptLenMap = AL_NumEncryptCreater.GetRandomAZMap();
            _numMapGroup = AL_NumEncryptCreater.GetRandomNumMapGroup();
            _xorKey = AL_NumEncryptCreater.GetRandomXorKey(out _numXorKey);
        }
        
        /// <summary>
        /// 获取26字母对应的数字【非 0~9 的映射表】,只在读取加密数据长度时候使用
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        protected int SignelToNum_26(char letter)
        {
            return _encryptLenMap.IndexOf(letter);
        }
        
        protected char NumToSignel_26(int index)
        {
            return _encryptLenMap[index];
        }
        
        public int GetYhNum(int num)
        {
            return num ^ _numXorKey;
        }
        
        /// <summary>
        /// 个位数字转映射符号 0~ 9
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        protected char Map_SingleNumToSignel(List<char> numMap, int num)
        {
            return numMap[num];
        }

        /// <summary>
        /// 映射符号转个位数字
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="signel"></param>
        /// <returns></returns>
        protected bool Map_TrySignelToSingleNum(List<char> numMap, char signel, out int num)
        {
            if (numMap.Contains(signel))
            {
                num = numMap.IndexOf(signel);
                return true;
            }
            else
            {
                num = 1;
                return false;
            }
        }
        
        /// <summary>
        /// 一串数字转映射字符串
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="num"></param>
        /// <param name="withSignel"></param>
        /// <returns></returns>
        protected string Map_NumToStr(List<char> numMap, int num, bool withSignel = false)
        {
            var numstr = num.ToString();
            var sb = new StringBuilder();
            if (withSignel)
            {
                var sig = num < 0 ? numMap[0] : numMap[1];//index 为0时候是负数 1时候是正数 标记
                sb.Append(sig);
            }
            for (int i = num < 0 ? 1 : 0; i < numstr.Length; i++)
            {
                int index = (int) Char.GetNumericValue(numstr[i]);
                sb.Append(numMap[index]);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 映射字符串转一串数字
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="encryptStr"></param>
        /// <param name="withSignel"></param> 是否首位带符号
        /// <returns></returns>
        protected bool Map_TryEncryptStrToNum(List<char> numMap, string encryptStr, out int num, bool withSignel = false)
        {
            num = 0;
            var multiple = 1;
            var firstIndex = withSignel ? 1 : 0;
            for (int i = encryptStr.Length - 1; i >= firstIndex; i--)
            {
                var signel = encryptStr[i];
                if (!numMap.Contains(signel)) return false;
                num += (numMap.IndexOf(signel) * multiple);
                multiple *= 10;
            }
            if (withSignel)
            {
                if (encryptStr[0] == numMap[0])
                {
                    num *= -1;
                }
                else if (encryptStr[0] != numMap[1])
                {
                    AL_FileLog.LogError("解析加密数字出错：首位没有携带符号信息");
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 非负数字字符串转映射字符串
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="numstr"></param>
        /// <returns></returns>
        protected string Map_NumStrToStr(List<char> numMap, string numstr)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < numstr.Length; i++)
            {
                int index = (int) Char.GetNumericValue(numstr[i]);
                sb.Append(numMap[index]);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 映射字符串转一串数字字符串
        /// </summary>
        /// <param name="numMap"></param>
        /// <param name="encryptStr"></param>
        /// <param name="num"></param>
        /// <param name="withSignel"></param>
        /// <returns></returns>
        protected bool Map_TryEncryptStrToNumStr(List<char> numMap, string encryptStr, out string num)
        {
            var sb = new StringBuilder();
            num = "";
            for (int i = 0; i < encryptStr.Length; i++)
            {
                if (!Map_TrySignelToSingleNum(numMap, encryptStr[i], out int singleNum)) return false;
                sb.Append(singleNum);
            }
            num = sb.ToString();
            return true;
        }
        
        
        protected bool TryGetStrSignel(string encryptStr, int index, out char signel)
        {
            if (encryptStr.Length > index)
            {
                signel = encryptStr[index];
                return true;
            }
            else
            {
                signel = 'a';
                return false;
            }
        }
        
        protected bool TryGetStrSubstring(string encryptStr, int startIndex, int len, out string outStr)
        {
            if (len > 0 && encryptStr.Length >= startIndex + len)
            {
                outStr = encryptStr.Substring(startIndex, len);
                return true;
            }
            else
            {
                outStr = "";
                return false;
            }
        }
        
        protected bool TryGetStrSubstring(string encryptStr, int startIndex, out string outStr)
        {
            if (encryptStr.Length > startIndex)
            {
                outStr = encryptStr.Substring(startIndex);
                return true;
            }
            else
            {
                outStr = "";
                return false;
            }
        }
        
        /// <summary>
        /// 获取index下 26字母对应的数字【非 0~9 的映射表】
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        protected bool TryDecryptCurSingleNum(string encryptStr, int index, out int singleNum)
        {
            singleNum = 0;
            if (!TryGetStrSignel(encryptStr, index, out char signel)) return false;
            singleNum = SignelToNum_26(signel);
            return true;
        }
        
        protected string MD5Encrypt64(string str, int length)
        {
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Convert.ToBase64String(s).Substring(0, length);
        }

        public abstract bool TryDecrypt(string encryptStr, out int outNum);

        public abstract string Encrypt(int num);

    }
}