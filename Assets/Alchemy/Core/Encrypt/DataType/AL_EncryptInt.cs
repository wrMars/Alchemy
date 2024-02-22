/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;

namespace Alchemy
{
    /// <summary>
    /// 注意【频繁调用时候】，用【临时的】int来记录下值而不是每次都直接当成int来用，这样可以省下性能（每次获取值都要解析）
    /// 相比于所在类继承于AL_NumEncryptData<AL_NumEncryptSingleMgr>的做法，直接使用AL_EncryptInt代替int来用的性能会稍差
    /// 可以当成int来用，但是注意，不支持序列化
    /// </summary>
    public class AL_EncryptInt:AL_NumEncryptData<AL_NumEncryptSingleMgr>, IFormattable, IEquatable<AL_EncryptInt>, IComparable<AL_EncryptInt>, IComparable<int>, IComparable
    {
        public int Value => GetEncryptValue(null);

        public AL_EncryptInt(int value)
        {
            SetEncryptValue(null, value);
        }
        
        #region operators, overrides, interface implementations
        public static implicit operator AL_EncryptInt(int value)
        {
            return new AL_EncryptInt(value);
        }

        public static implicit operator int(AL_EncryptInt myInt)
        {
            return myInt.Value;
        }
        
        public static AL_EncryptInt operator ++(AL_EncryptInt input)
        {
            return Increment(input, 1);
        }
        
        public static AL_EncryptInt operator --(AL_EncryptInt input)
        {
            return Increment(input, -1);
        }
        
        private static AL_EncryptInt Increment(AL_EncryptInt input, int increment)
        {
            input.SetEncryptValue(null, input.Value + increment);
            return input;
        }
        
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public string ToString(string format)
        {
            return Value.ToString(format);
        }
        
        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }
        
        public string ToString(string format, IFormatProvider provider)
        {
            return Value.ToString(format, provider);
        }
        
        public override bool Equals(object obj)
        {
            return obj is AL_EncryptInt && Equals((AL_EncryptInt)obj);
        }
        
        public bool Equals(AL_EncryptInt obj)
        {
            if (obj == null) return false;
            return Value.Equals(obj.Value);
        }
        
        public int CompareTo(AL_EncryptInt other)
        {
            return Value.CompareTo(other.Value);
        }
        
        public int CompareTo(int other)
        {
            return Value.CompareTo(other);
        }
        
        public int CompareTo(object obj)
        {
#if !ACTK_UWP_NO_IL2CPP
            return Value.CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is int)) throw new ArgumentException("Argument must be int");
			return CompareTo((int)obj);
#endif
        }
        #endregion
    }
}