//------------------------------------------------------------
// File : MathTool.cs
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public static class MathTool
    {
        public static long Min(long a, long b) => a < b ? a : b;
        public static long Max(long a, long b) => (double)a > (double)b ? a : b;
        public static List<int> GetLongToInt(this long a)
        {
            List<int> list = new List<int>();
            while (a > int.MaxValue)
            {
                a -= int.MaxValue;
                list.Add(int.MaxValue);
            }

            list.Add((int)a);
            return list;
        }

        public static long Clamp(long value, long min, long max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        public static double Clamp01(double value)
        {
            if ((double)value < 0.0)
                return 0.0f;
            return (double)value > 1.0 ? 1f : value;
        }
        public static string TranslateNum(long num)
        {
            string result = num.ToString("0");
            if (num > 999 && num <= 999999)
            {
                result = string.Format("{0:0.#}K", num * 0.001f);
            }
            else if (num > 999999 && num <= 999999999)
            {
                result = string.Format("{0:0.#}M", num * 0.000001f);
            }
            else if (num > 999999999 && num <= 999999999999)
            {
                result = string.Format("{0:0.#}B", num * 0.000000001f);
            }
            else if (num > 999999999999)
            {
                result = string.Format("{0:0.#}T", num * 0.000000000001f);
            }
            return result;
        }

        public static string TranslateNumNoDecial(long num)
        {
            string result = num.ToString("0");
            if (num > 999 && num <= 999999)
            {
                result = string.Format("{0:0}K", num * 0.001f);
            }
            else if (num > 999999 && num <= 999999999)
            {
                result = string.Format("{0:0}M", num * 0.000001f);
            }
            else if (num > 999999999 && num <= 999999999999)
            {
                result = string.Format("{0:0}B", num * 0.000000001f);
            }
            else if (num > 999999999999)
            {
                result = string.Format("{0:0}T", num * 0.000000000001f);
            }
            return result;
        }

        public static Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
        {
            float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
            if (tmp == 0)
            {
                // No solution!
                found = false;
                return Vector2.zero;
            }

            float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
            found = true;

            return new Vector2(
                B1.x + (B2.x - B1.x) * mu,
                B1.y + (B2.y - B1.y) * mu
            );
        }

        /// <summary>
        /// 计算射线和面的交点 
        /// 会有一定误差 ， 浮点数计算没有办法
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="normal">面的法线</param>
        /// <param name="Point">面上的一点</param>
        /// <param name="ret">交点</param>
        /// <returns>线和面是否相交</returns>
        public static bool GetIntersectionOfRayAndFace(Ray ray, Vector3 normal, Vector3 Point, out Vector3 ret)
        {
            if (Vector3.Dot(ray.direction, normal) == 0)
            {
                //如果平面法线和射线垂直 则不会相交
                ret = Vector3.zero;
                return false;
            }
            Vector3 Forward = normal;
            Vector3 Offset = Point - ray.origin; //获取线的方向
            float DistanceZ = Vector3.Angle(Forward, Offset); //计算夹角
            DistanceZ = Mathf.Cos(DistanceZ / 180f * Mathf.PI) * Offset.magnitude; //算点到面的距离
            DistanceZ /= Mathf.Cos(Vector3.Angle(ray.direction, Forward) / 180f * Mathf.PI); //算点沿射线到面的距离
            ret = ray.origin + ray.direction * DistanceZ; //算得射线和面的交点
            return true;
        }

        public static string DisplayWithSuffix(int num)
        {
            string number = num.ToString();
            if (number.EndsWith("11")) return number + "th";
            if (number.EndsWith("12")) return number + "th";
            if (number.EndsWith("13")) return number + "th";
            if (number.EndsWith("1")) return number + "st";
            if (number.EndsWith("2")) return number + "nd";
            if (number.EndsWith("3")) return number + "rd";
            return number + "th";
        }
        public static List<string> romanNumerals = new List<string>() { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        public static List<int> numerals = new List<int>() { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

        public static string ToRomanNumeral(int number)
        {
            var romanNumeral = string.Empty;
            while (number > 0)
            {
                // find biggest numeral that is less than equal to number
                var index = numerals.FindIndex(x => x <= number);
                // subtract it's value from your number
                number -= numerals[index];
                // tack it onto the end of your roman numeral
                romanNumeral += romanNumerals[index];
            }
            return romanNumeral;
        }
    }
}