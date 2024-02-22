using System;
using System.Text;
using Random = UnityEngine.Random;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_NumEncryptComplex:AL_NumEncryptBase
    {
        public override bool TryDecrypt(string encryptStr, out int outNum)
        {
            //字段读取
            outNum = 0;
            if (string.IsNullOrEmpty(encryptStr)) return false;
            //seed
            var tempIndex = 0;
            if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int seed)) return false;
            var seedMap = _numMapGroup[seed];
            //seedNum
            tempIndex += 1;
            if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int seedNum)) return false;
            var numMap = _numMapGroup[seedNum];
            // //seedID
            // tempIndex += 1;
            // if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int seedID)) return false;
            // var idMap = _numMapGroup[seedID];
            //符号信息
            tempIndex += 1;
            if (!TryGetStrSignel(encryptStr, tempIndex, out char signel)) return false;
            if (!Map_TrySignelToSingleNum(seedMap, signel, out int symbol)) return false;
            bool isNegative = symbol == 0;//symbol 0：-  1：+

            //numEncrypt
            tempIndex += 1;
            if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int numLen)) return false;
            tempIndex += 1;
            if (!TryGetStrSubstring(encryptStr, tempIndex, numLen, out string numEncrypt)) return false;
            tempIndex += numLen;
            
            // //idEncrypt
            // if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int idLen)) return false;
            // tempIndex += 1;
            // if (!TryGetStrSubstring(encryptStr, tempIndex, idLen, out string idEncrypt)) return false;
            // tempIndex += idLen;
            
            //key
            if (!TryDecryptCurSingleNum(encryptStr, tempIndex, out int keyLen)) return false;
            tempIndex += 1;
            if (!TryGetStrSubstring(encryptStr, tempIndex, keyLen, out string keyEncrypt)) return false;
            tempIndex += keyLen;
            
            
            //解密判断合法
            // if (!Map_TryEncryptStrToNum(idMap, idEncrypt, out int idNum)) return false;
            // if (idNum != _guid) return false;
            if (!Map_TryEncryptStrToNum(numMap, numEncrypt, out outNum)) return false;
            
            var sb = new StringBuilder();
            sb.Append(seed);
            sb.Append(seedNum);
            // sb.Append(seedID);
            sb.Append(symbol);
            sb.Append(outNum);
            // sb.Append(idNum);

            if (!CompareEncryptKey(sb.ToString(), keyEncrypt)) return false;
            // if (MD5Encrypt64(sb.ToString()) != md5sub) return false;
            outNum = GetYhNum(outNum);
            if (isNegative) outNum *= -1;
            return true;
        }
        
        /// <summary>
        /// 加密
        /// seed + seedNum + seedID + 符号信息(0/1) + len + numEncrypt(无符号、绝对值异或后的） + "," + MD5
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public override string Encrypt(int num)
        {
            var md5Str = new StringBuilder();//将对应的数字逐个排在一起
            //seed
            var randomIndex = Random.Range(0, 25);
            var seed = NumToSignel_26(randomIndex);
            var seedMap = _numMapGroup[randomIndex];
            md5Str.Append(randomIndex);
            //seedNum
            randomIndex = Random.Range(0, 25);
            var seedNum = NumToSignel_26(randomIndex);
            var numMap = _numMapGroup[randomIndex];
            md5Str.Append(randomIndex);
            // //seedID
            // randomIndex = Random.Range(0, 25);
            // var seedID = NumToSignel_26(randomIndex);
            // var idMap = _numMapGroup[randomIndex];
            // md5Str.Append(randomIndex);
            //符号信息
            var negativeIndex = num < 0 ? 0 : 1;
            md5Str.Append(negativeIndex);
            var negativeSignel = seedMap[negativeIndex];

            var absNum = Math.Abs(num);
            var numEncrypt = GetYhNum(absNum);
            var numEncryptStr = Map_NumToStr(numMap, numEncrypt);
            var numEncryptLen = NumToSignel_26(numEncryptStr.Length);
            md5Str.Append(numEncrypt);
            // md5Str.Append(_guid);

            // var idEncryptStr = Map_NumToStr(idMap, _guid);
            // var idEncryptLen = NumToSignel_26(idEncryptStr.Length);

            var key = GetEncryptKey(md5Str.ToString());
            var keyEncryptLen = NumToSignel_26(key.Length);
            
            var reStr = new StringBuilder();
            reStr.Append(seed);
            reStr.Append(seedNum);
            // reStr.Append(seedID);
            reStr.Append(negativeSignel);
            reStr.Append(numEncryptLen);
            reStr.Append(numEncryptStr);
            // reStr.Append(idEncryptLen);
            // reStr.Append(idEncryptStr);
            reStr.Append(keyEncryptLen);
            reStr.Append(key);
            // reStr.Append($",{GetEncryptKey(md5Str.ToString())}");
            
            return reStr.ToString();
        }
        
        private bool CompareEncryptKey(string numStr, string encryptKeyStr)
        {
            return GetEncryptKey(numStr) == encryptKeyStr;
        }
        
        private string GetEncryptKey(string str)
        {
            return ByteXor(str);
        }
        
        private string ByteXor(string resource)
        {
            byte[] data = Encoding.Unicode.GetBytes(resource);
            byte[] re = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                re[i] = Convert.ToByte(data[i] ^ _xorKey[i % _xorKey.Length]);
            }
            return Encoding.Unicode.GetString(re);
        }
    }
}