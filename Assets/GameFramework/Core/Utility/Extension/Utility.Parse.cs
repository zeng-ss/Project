//------------------------------------------------------------
// File : Utility.Parse.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 数据类型装换
//------------------------------------------------------------

namespace GameFramework
{
    public static partial class Utility
    {
        public static class Parse
        {
            public static int parseObjInt(object obj, int defaultValue = 0)
            {
                if (obj == null)
                {
                    return defaultValue;
                }

                return parseInt(obj.ToString(), defaultValue);
            }
            public static byte parseByte(string text, byte defaultValue = 0)
            {
                byte _return = 0;
                if (byte.TryParse(text, out _return))
                    return _return;
                else
                    return defaultValue;
            }
            public static int parseInt(string text, int defaultValue = 0)
            {
                int _return = 0;
                if (int.TryParse(text, out _return))
                    return _return;
                else
                    return defaultValue;
            }

            public static float parseFloat(string text, float defaultValue = 0)
            {
                float _return = 0;
                if (float.TryParse(text, out _return))
                    return _return;
                else
                    return defaultValue;
            }

            public static long parseLong(string text, long defaultValue = 0)
            {
                long _return = 0;
                if (long.TryParse(text, out _return))
                    return _return;
                else
                    return defaultValue;
            }
            public static bool parseBool(string text, bool defaultValue = false)
            {
                bool _return = false;
                if (bool.TryParse(text, out _return))
                    return _return;
                else
                    return defaultValue;
            }
        }

    }
}