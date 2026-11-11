using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public class DataTableTypeCheck : IDataTableChecker
    {
        public void Check()
        {
            Assembly[] s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in s_Assemblies)
            {
                var baseType = typeof(DataRowBase);

                var subTypes = assembly.GetTypes()
                    .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in subTypes)
                {
                    DataTableBase dataTableBase = DataTableCheck.GetDataTable(type);

                }
            }
        }

        private void CheckType<T>(Type type, DataTableBase dataTableBase, Action<T, int> customCheckAction)
        {
            Type customDataTableType = DataTableCheck.GetCustomDataTable(type);

            // 获取 GetAllDataRows 方法
            MethodInfo method = customDataTableType.GetMethod(
                "GetAllDataRows",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { }, // 无参数
                null
            );

            if (method == null) return;

            // 获取所有公共实例字段
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.PropertyType == typeof(T));

            object result = method.Invoke(dataTableBase, null);

            if (result is not IEnumerable enumerable) return;

            int index = 0;
            foreach (var tempItem in enumerable)
            {
                index++;
                foreach (var propertyInfo in propertyInfos)
                {
                    // 获取字段值
                    T items = (T)propertyInfo.GetValue(tempItem);

                    // 遍历数组
                    customCheckAction?.Invoke(items, index);
                }
            }
        }
    }
}