/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public abstract class AL_NumEncryptMgrBase
    {
        public abstract bool TryGetValue(string attrFunName, out int outNum, int defaultNum = 0);
        public abstract void TrySetValue(string attrFunName, int value);
    }
}