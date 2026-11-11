using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// 显式声明面板预制体在 Addressables 中的地址；未标注则回退类名。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PanelPathAttribute : Attribute
    {
        public string Path { get; }

        public PanelPathAttribute(string path)
        {
            Path = path;
        }
    }

    /// <summary>面板地址解析器。</summary>
    public static class PanelPathResolver
    {
        private static readonly Dictionary<Type, string> Cache = new();

        public static string Resolve<T>() where T : UIFormBase => Resolve(typeof(T));

        public static string Resolve(Type panelType)
        {
            if (Cache.TryGetValue(panelType, out var cached)) return cached;

            var attribute = (PanelPathAttribute)Attribute.GetCustomAttribute(panelType, typeof(PanelPathAttribute));
            string path = attribute != null ? attribute.Path : panelType.Name;
            Cache[panelType] = path;
            return path;
        }
    }
}
