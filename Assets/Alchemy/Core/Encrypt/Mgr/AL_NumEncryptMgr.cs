using System.Collections.Generic;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 管理类内多个属性需要加密的情况
    /// </summary>
    public class AL_NumEncryptMgr:AL_NumEncryptMgrBase
    {
        //num_fun_name -> num_encrypt_str
        private Dictionary<string, string> _numToEncryptStrMap;
        
        //num_fun_name -> num
        // private Dictionary<string, int> _originalNumNote = new Dictionary<string, int>();//这个是为了运算性能做的缓存处理；可能会被内存搜查作弊找到并修改，不过没事，加密数据不是从这里拿;非加密的可以从这里拿

        /// <summary>
        /// 只能在get set 中使用该方法
        /// </summary>
        /// <param name="attrFunName"></param>
        /// <param name="outNum"></param>
        /// <param name="defaultNum"></param>
        /// <returns></returns>
        public override bool TryGetValue(string attrFunName, out int outNum, int defaultNum = 0)
        {
            // if (AL_NumEncrypt.UseEncrypt)
            // {
                _numToEncryptStrMap ??= new Dictionary<string, string>();
                bool had = _numToEncryptStrMap.TryGetValue(attrFunName, out string encryptStr);
                if (had && AL_NumEncrypt.TryDecrypt(encryptStr, out outNum))
                {
                    return true;
                }

                if (!had)
                {
                    UnityEngine.Debug.LogWarning($"{attrFunName} 缺少的加密数据(读取前还没有赋值)，使用默认数据{defaultNum}");
                    // AL_LogTool.LogOnlyEditor($"{attrFunName} 缺少的加密数据(读取前还没有赋值)，使用默认数据{defaultNum}");
                }
                else
                {
                    AL_FileLog.LogError($"{attrFunName} 解密失败");
                }
                outNum = defaultNum;
                return false;
            // }
            // else
            // {
            //     var re = _originalNumNote.TryGetValue(attrFunName, out outNum);
            //     if (!re) outNum = defaultNum;
            //     return re;
            // }
        }
        
        /// <summary>
        /// 只能在get set 中使用该方法
        /// </summary>
        /// <param name="attrFunName"></param>
        /// <param name="value"></param>
        public override void TrySetValue(string attrFunName, int value)
        {
            // if (!_originalNumNote.TryGetValue(attrFunName, out int note))
            // {
                SetValue(attrFunName, value);
            // }
            // else if (note != value)
            // {
            //     SetValue(attrFunName, value);
            // }
        }
        
        private void SetValue(string attrFunName, int value)
        {
            // _originalNumNote[attrFunName] = value;
            // if (AL_NumEncrypt.UseEncrypt)
            // {
                _numToEncryptStrMap ??= new Dictionary<string, string>();
                _numToEncryptStrMap[attrFunName] = AL_NumEncrypt.Encrypt(value);
            // }
        }

        public void Destroy()
        {
            if (_numToEncryptStrMap != null)
            {
                _numToEncryptStrMap.Clear();
                _numToEncryptStrMap = null;
            }
            // if (_originalNumNote != null)
            // {
            //     _originalNumNote.Clear();
            //     _originalNumNote = null;
            // }
        }
    }
}