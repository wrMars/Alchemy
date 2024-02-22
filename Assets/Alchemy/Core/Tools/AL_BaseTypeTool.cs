/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using UnityEngine;

namespace Alchemy
{
    public class AL_BaseTypeTool
    {
        public static T GetEnumByIndex<T>(int index) where T : Enum
        {
            T[] enumValues = (T[])Enum.GetValues(typeof(T));

            if (index >= 0 && index < enumValues.Length)
            {
                return enumValues[index];
            }

            throw new IndexOutOfRangeException("Index out of range for enum " + typeof(T).Name);
        }

        public static T GetEnumByString<T>(string key) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), key);
        }

        public static bool CheckLegal(Vector3 v3)
        {
            return !(float.IsNaN(v3.x) || float.IsNaN(v3.y) || float.IsNaN(v3.z));
        }
    }
}