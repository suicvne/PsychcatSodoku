using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions
{
    [Serializable]
    public struct StringToEnum<T> where T : Enum
    {
        public string _StringName;
        public T _EnumValue;
    }

    public class StringToEnumMap<T> : ScriptableObject where T : Enum
    {
        [SerializeField] List<StringToEnum<T>> StringToEnumLookup;

        public List<StringToEnum<T>> GetList() => StringToEnumLookup;

        public T GetEnumByString(string value)
        {
            foreach (var lookup in StringToEnumLookup)
            {
                if (lookup._StringName == value)
                {
                    return lookup._EnumValue;
                }
            }

            return default(T);
        }

        public string GetStringByEnum(T value)
        {
            foreach(var lookup in StringToEnumLookup)
            {
                if(EqualityComparer<T>.Default.Equals(value, lookup._EnumValue))
                {
                    return lookup._StringName;
                }
            }

            return null;
        }
    }
}