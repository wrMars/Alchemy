/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;

namespace Alchemy
{
    public class AL_ConvertTool
    {
        public static object Str2Enum(Type enumType, string val)
        {
            return Enum.Parse(enumType, val);
        }
    }
}