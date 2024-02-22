using System;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public abstract class AL_NumEncryptData<T>:IAL_NumEncryptData<T> where T : AL_NumEncryptMgrBase, new()
    {
        public T EncryptMgr { get; set; }

        public int GetEncryptValue(string attrFunName,  int defaultNum = 0)
        {
            EncryptMgr ??= new T();
            EncryptMgr.TryGetValue(attrFunName, out int re, defaultNum);
            return re;
        }

        public void SetEncryptValue(string attrFunName, int value)
        {
            EncryptMgr ??= new T();
            EncryptMgr.TrySetValue(attrFunName, value);
        }
    }
    
    public interface IAL_NumEncryptData<T> where T : AL_NumEncryptMgrBase, new()
    {
        public T EncryptMgr { get; set; }
        public int GetEncryptValue(string attrFunName, int defaultNum = 0);
        public void SetEncryptValue(string attrFunName, int value);
    }
}