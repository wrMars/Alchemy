using System;
using System.Collections.Generic;
using System.Text;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public class AL_NumEncryptSimple:AL_NumEncryptBase
    {
        private string[] _arrEncrypt;
        public override bool TryDecrypt(string encryptStr, out int outNum)
        {
            outNum = 0;
            _arrEncrypt = encryptStr.Split(',');
            if (_arrEncrypt?.Length == 2)
            {
                if (!TryDecryptOneStr(_arrEncrypt[0], out outNum)) return false;
                if (!TryDecryptOneStr(_arrEncrypt[1], out var outYhNum)) return false;
                return GetYhNum(outNum) == outYhNum;
            }
            return false;
        }

        private bool TryDecryptOneStr(string encryptStr, out int outNum)
        {
            //字段读取
            outNum = 0;
            if (string.IsNullOrEmpty(encryptStr)) return false;
            //seed
            int curIndex = 0;
            if (!TryDecryptCurSingleNum(encryptStr, curIndex, out int seed)) return false;
            var seedMap = _numMapGroup[seed];
            //符号信息
            curIndex += 1;
            if (!TryGetStrSignel(encryptStr, curIndex, out char signel)) return false;
            if (!Map_TrySignelToSingleNum(seedMap, signel, out int symbol)) return false;
            bool isNegative = symbol == 0;//symbol 0：-  1：+
            //numEncrypt
            curIndex += 1;
            if (!TryDecryptCurSingleNum(encryptStr, curIndex, out int numLen)) return false;
            curIndex += 1;
            if (!TryGetStrSubstring(encryptStr, curIndex, numLen, out string numEncrypt)) return false;
            curIndex += numLen;
            
            if (!Map_TryEncryptStrToNum(seedMap, numEncrypt, out outNum)) return false;
            if (isNegative) outNum *= -1;
            return true;
        }

        public override string Encrypt(int num)
        {
            return $"{EncryptOneNum(num)},{EncryptOneNum(GetYhNum(num))}";
        }

        private Random _random;
        private string EncryptOneNum(int num)
        {
            //seed
            _random ??= new Random();
            int randomIndex = _random.Next(26);
            var seed = NumToSignel_26(randomIndex);
            var seedMap = _numMapGroup[randomIndex];
            
            //符号信息
            var negativeIndex = num < 0 ? 0 : 1;
            var negativeSignel = seedMap[negativeIndex];
            
            //num
            var absNum = Math.Abs(num);
            var numEncryptStr = Map_NumToStr(seedMap, absNum);
            var numEncryptLen = NumToSignel_26(numEncryptStr.Length);
            
            var reStr = new StringBuilder();
            reStr.Append(seed);
            reStr.Append(negativeSignel);
            reStr.Append(numEncryptLen);
            reStr.Append(numEncryptStr);
            return reStr.ToString();
        }
    }
}