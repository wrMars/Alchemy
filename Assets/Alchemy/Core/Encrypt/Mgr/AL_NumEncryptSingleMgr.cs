/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 管理类内单个属性需要加密的情况
    /// </summary>
    public class AL_NumEncryptSingleMgr:AL_NumEncryptMgrBase
    {
        private string _numToEncryptStr;
        // private int _originalNum;
        
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
                if (AL_NumEncrypt.TryDecrypt(_numToEncryptStr, out outNum))
                {
                    return true;
                }
                AL_FileLog.LogErrorLater($"解析内存加密失败，str={_numToEncryptStr}");
                outNum = defaultNum;
                return false;
            // }
            // else
            // {
            //     outNum = _originalNum;
            //     return true;
            // }
        }
        
        /// <summary>
        /// 只能在get set 中使用该方法
        /// </summary>
        /// <param name="attrFunName"></param>
        /// <param name="value"></param>
        public override void TrySetValue(string attrFunName, int value)
        {
            // if (_originalNum == 0)
            // {
                SetValue(attrFunName, value);
            // }
            // else if (_originalNum != value)
            // {
            //     SetValue(attrFunName, value);
            // }
        }
        
        private void SetValue(string attrFunName, int value)
        {
            // _originalNum = value;
            // if (AL_NumEncrypt.UseEncrypt)
            // {
                _numToEncryptStr = AL_NumEncrypt.Encrypt(value);
            // }
        }
    }
}