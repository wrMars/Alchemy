/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Alchemy
{
    public static class AL_ReflectTool
    {
        public static dynamic GetDynamicInstance(Type genericType, Type[] typeArguments)
        {
            Type constructedType = genericType.MakeGenericType(typeArguments);
            return Activator.CreateInstance(constructedType);
        }
        
        public static List<Type> GetAllAttributeUserInAssembly(Type attributeType)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types
                .Where(type => Attribute.IsDefined(type, attributeType))
                .ToList();
        }

        public static List<Type> GetAllInAssembly(Type t)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(type => type.IsSubclassOf(t)).ToList();
        }

        public static Type GetAttributeUserInAssembly(Type attributeType, string typeName)
        {
            return GetAllAttributeUserInAssembly(attributeType)?.Find(type => type.Name == typeName);
        }

        public static void CopyObject(object source, object target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            FieldInfo[] fields = sourceType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] properties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                FieldInfo targetField = targetType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public);
                if (targetField != null)
                {
                    object value = field.GetValue(source);
                    targetField.SetValue(target, value);
                }
            }

            foreach (PropertyInfo property in properties)
            {
                PropertyInfo targetProperty = targetType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    object value = property.GetValue(source);
                    targetProperty.SetValue(target, value);
                }
            }
        }

        public static bool IsArrayOrList(this Type type, out Type elementType)
        {
            elementType = null;
            if (type == null) return false;
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GenericTypeArguments[0];
                return true;
            }
            return false;
        }

        public static FieldInfo[] GetMonoSerializeField(Type monoType)
        {
            var publicFields = monoType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var otherFields = monoType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            List<FieldInfo> privateFields = new List<FieldInfo>();
            foreach (var finfo in otherFields)
            {
                if (finfo.IsDefined(typeof(SerializeField)))
                {
                    privateFields.Add(finfo);
                }
            }
            return privateFields.ToArray().Concat(publicFields).ToArray();
        }
    }
}