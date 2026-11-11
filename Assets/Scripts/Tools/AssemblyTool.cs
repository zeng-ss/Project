using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CasualGame;
using Newtonsoft.Json;
using UnityEngine;

public static class AssemblyTool
{
    private static FieldInfo[] GetAllFields(Type type)
    {
        // 获取所有字段，包括公共、非公共、实例和静态字段
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        return fields;
    }

    public static List<string> GetAllFiledValues(Type type)
    {
        FieldInfo[] fieldInfos = GetAllFields(type);

        List<string> keys = new List<string>();

        foreach (var fieldInfo in fieldInfos)
        {
            if (fieldInfo.GetValue(null) is string text)
            {
                keys.Add(text);
            }
        }

        return keys;
    }
}
