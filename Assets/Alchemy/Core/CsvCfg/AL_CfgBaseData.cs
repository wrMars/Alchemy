/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alchemy
{
    [AL_CfgVo]
    public class AL_BaseCfgVo
    {
        public virtual void CompleteParse(){}
        public void ParseAttr(List<string> attrNames, List<string> attrVals)
        {
            var fieldInfos = GetType().GetFields();
            int index;
            foreach (var info in fieldInfos)
            {
                index = attrNames.IndexOf(info.Name);
                if (index >= 0 && info.FieldType.IsPublic)
                {
                    if (info.FieldType == typeof(string))
                    {
                        info.SetValue(this, attrVals[index]);
                    }
                    else if (info.FieldType.IsEnum)
                    {
                        if (Enum.IsDefined(info.FieldType, attrVals[index]))
                        {
                            info.SetValue(this, Enum.Parse(info.FieldType, attrVals[index]));
                        }
                        else
                        {
                            throw new Exception($"解析失败，对应的枚举值找不到：{info.FieldType} name:{attrVals[index]}");
                        }
                    }
                    else
                    {
                        try
                        {
                            info.SetValue(this,Convert.ChangeType(attrVals[index], info.FieldType));
                        }
                        catch (Exception e)
                        {
                            AL_LogTool.LogOnlyEditor(e.Message);
                            AL_LogTool.LogOnlyEditor($"结构含不支持解析类型：{info.FieldType} name:{info.Name}");
                            throw;
                        }
                        
                    }
                }
            }
            CompleteParse();
        }
    }
    
    [AL_CfgData]
    public abstract class AL_BaseCfgVos:ScriptableObject
    {
        public abstract IList Vos { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AL_CfgVoAttribute : Attribute
    {
        public static List<Type> GetAllUserInAssembly()
        {
            return AL_ReflectTool.GetAllAttributeUserInAssembly(typeof(AL_CfgVoAttribute));
        }

        public static Type GetUserInAssembly(string typeName)
        {
            return GetAllUserInAssembly()?.Find(type => type.Name == typeName);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AL_CfgDataAttribute : Attribute
    {
        public static List<Type> GetAllUserInAssembly()
        {
            return AL_ReflectTool.GetAllAttributeUserInAssembly(typeof(AL_CfgDataAttribute));
        }

        public static Type GetUserInAssembly(string typeName)
        {
            return GetAllUserInAssembly()?.Find(type => type.Name == typeName);
        }
    }
}