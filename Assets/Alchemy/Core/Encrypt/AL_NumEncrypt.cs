using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 
    /// 【使用说明】
    ///     1、敏感数据类(多属性需要加密)继承于 AL_NumEncryptData 或者对应的接口 ；对应敏感数据修改成get；set；模式，然后统一用下面的方式来操作值（例子）：
    ///         以下是_userData中属性：
    ///         public int Gold { get=>GetEncryptValue("xxxx");private set=>SetEncryptValue("xxxx",value); }
    ///         如果是单属性加密的，可以看下AL_NumEncryptFilterData 或者对应的接口;
    ///         AL_EncryptInt可以直接替换int来使用，但是性能稍差于上述对类操作的做法
    ///     2、如果项目有不可避免的巨量频繁设置读取敏感数据，可以设置UseTemp = true;来用空间换取运算量；一帧1000次以内的都不需要这样处理，否则性能反而会差。
    ///
    ///     改数据上层(set)
    ///     1、由服务器发回来的数据来设置的，不用管
    ///     2、不是直接=xxx，而是+=、-=、=a+b这些样式的，不用管；这种一般不可以通过改内存作弊，不然他就得筛选到差值，然后修改；并且这种差值一般都是运算时才会出现的数值，不会是存在内存中固定位置的某些数据。
    /// 
    /// 【注意】
    ///     1、所有敏感数据都要使用框架处理，尤其是设置值的时候，确保设置进来的值是正确的，【避免有别的类变量或者静态变量缓存了，然后再通过那个错误数据设置进来】，这种变量容易被内存修改，导致加密也没用
    ///         即还要改数据上层
    ///     2、目前仅支持长度为25（包含）以内的数据进行加解密
    ///     3、使用时，尽量用临时变量缓存数据，但是不要用类变量或者静态变量缓存；这样可以减少运算以及避免内存搜查
    ///     4、仅当数据在get set 中才能使用该框架
    ///     5、关注所有的敏感数据的set，保证整个流程的set都实际操作的是加密的
    /// </summary>
    
    
    public static class AL_NumEncryptCreater
    {
        public static List<char> GetRandomAZMap()
        {
            List<char> letters = Enumerable.Range('a', 26).Select(x => (char)x).ToList();

            System.Random random = new System.Random();
            for (int i = letters.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = letters[i];
                letters[i] = letters[j];
                letters[j] = temp;
            }
            return letters;
        }

        public static List<char>[] GetRandomNumMapGroup()
        {
            List<char> azMap = Enumerable.Range('a', 26).Select(x => (char)x).ToList();
            List<HashSet<char>> maps = new List<HashSet<char>>();
            for (int i = 0; i < 26; i++)
            {
                maps.Add(GetOneRandomMap(maps, azMap, 10));
            }
            return maps.Select(a => a.ToList()).ToArray();
        }

        private static HashSet<char> GetOneRandomMap(List<HashSet<char>> resList, List<char> azMap, int len)
        {
            System.Random random = new System.Random();
            for (int i = 0; i < len; i++)
            {
                var index = random.Next(i, azMap.Count);
                char temp = azMap[i];
                azMap[i] = azMap[index];
                azMap[index] = temp;
            }
            var re = new HashSet<char>(azMap.GetRange(0, len));
            var val = resList.Find(a => a.SetEquals(re));
            if (val != null)
            {
                return GetOneRandomMap(resList, azMap, len);
            }
            else
            {
                return re; 
            }
        }

        public static byte[] GetRandomXorKey(out int xorKeyNum)
        {
            List<char> azMap = Enumerable.Range('a', 26).Select(x => (char)x).ToList();
            var numStr = new StringBuilder();
            System.Random random = new System.Random();
            for (int i = 0; i < AL_NumEncrypt.RANDOM_YH_KEY_LEN; i++)
            {
                var index = random.Next(i, azMap.Count);
                numStr.Append(index);
                char temp = azMap[i];
                azMap[i] = azMap[index];
                azMap[index] = temp;
            }
            xorKeyNum = int.Parse(numStr.ToString());
            return Encoding.Unicode.GetBytes(azMap.GetRange(0, AL_NumEncrypt.RANDOM_YH_KEY_LEN).ToArray());
        }
    }

    public enum AL_NumEncryptMode
    {
        Simple,
        Complex
    }
    
    public class AL_NumEncrypt
    {
        // public const bool UseEncrypt = true;//是否使用加解密
        
        public const int RANDOM_YH_KEY_LEN = 3;
        private const AL_NumEncryptMode EncryptMode = AL_NumEncryptMode.Simple;

        //==================================================================

        private static AL_NumEncryptBase _numEncrypt;
        public static AL_NumEncryptBase NumEncrypt
        {
            get
            {
                _numEncrypt ??= CreateNumEncryptor();
                return _numEncrypt;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 游戏运行中不要调用，会改变密码库
        /// </summary>
        public static void UpdateNumEncryptor()
        {
            if (Application.isPlaying)
            {
                if (Application.isEditor) AL_FileLog.LogError("游戏运行过程中不允许修改密码本");
                return;
            }
            _numEncrypt = CreateNumEncryptor();
        }
#endif

        private static AL_NumEncryptBase CreateNumEncryptor()
        {
            switch (EncryptMode)
            {
                case AL_NumEncryptMode.Complex:
                    return new AL_NumEncryptComplex();
                case AL_NumEncryptMode.Simple:
                    return new AL_NumEncryptSimple();
            }
        }

        private static int curDecryptTime = 1;
        public static bool TryDecrypt(string encryptStr, out int outNum)
        {
            outNum = 0;
            curDecryptTime += 1;
            return NumEncrypt.TryDecrypt(encryptStr, out outNum);
        }

        private static int curEncryptTime = 1;
        public static string Encrypt(int num)
        {
            curEncryptTime += 1;
            return NumEncrypt.Encrypt(num);
        }
    }
}