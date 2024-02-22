/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy.ExtraClass
{
    public static class AL_ObjectExtra
    {
        public static bool IsDefaultOrEmpty<T>(this object obj, T data)
        {
            return data?.Equals(default(T)) ?? true;
        }
    }
}