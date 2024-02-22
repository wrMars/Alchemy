/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    /// <summary>
    /// 处理大批量体量小且需要过滤的数据 ，比如物品id，只加密其中某些id的物品的数量数据;即该类实例中有需要加密也有不需要加密的使用情况
    /// 使用：
    /// 1、IsEncrypt
    /// 2、EncryptInit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AL_NumEncryptFilterData<T>:IAL_NumEncryptFilterData<T>
    {
        public AL_NumEncryptSingleMgr EncryptMgr { get; set; }
        private int _originalNum;
        private T _filterKey;

        public T FilterKey
        {
            get => _filterKey;
            set => _filterKey = value;
        }

        private bool CheckIsEncrypt(out bool mgrInit)
        {
            mgrInit = false;
            if (EncryptMgr != null)
            {
                return true;
            }
            else
            {
                if (IsEncrypt(FilterKey))
                {
                    if (EncryptMgr == null)
                    {
                        EncryptMgr = new AL_NumEncryptSingleMgr();
                        mgrInit = true;
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public int GetEncryptValue(string attrFunName, T filterKey = default(T), int defaultNum = 0)
        {
            if (CheckIsEncrypt(out bool mgrInit))
            {
                //由于可能滞后初始化（数据先设置），所以首次初始化时候（先于SetEncryptValue），需要设置数值
                if (mgrInit)
                {
                    SetEncryptValue(attrFunName, _originalNum);
                    return _originalNum;
                }
                else
                {
                    EncryptMgr.TryGetValue(attrFunName, out int re, defaultNum);
                    return re;
                }
            }
            else
            {
                return _originalNum;
            }
        }
        public void SetEncryptValue(string attrFunName, int value, T filterKey = default(T))
        {
            if (CheckIsEncrypt(out bool mgrInit))
            {
                EncryptMgr.TrySetValue(attrFunName, value);
            }
            else
            {
                _originalNum = value;
            }
        }
        public abstract bool IsEncrypt(T filterKey);
    }

    public interface IAL_NumEncryptFilterData<T>
    {
        public AL_NumEncryptSingleMgr EncryptMgr { get; set; }
        public int GetEncryptValue(string attrFunName, T filterKey = default(T), int defaultNum = -9999);
        public void SetEncryptValue(string attrFunName, int value, T filterKey = default(T));
        /// <summary>
        /// 加密字段只在某些条件下生效时使用，比如只对某些id对应的值进行加密
        /// </summary>
        /// <returns></returns>
        public bool IsEncrypt(T filterKey);
    }
}