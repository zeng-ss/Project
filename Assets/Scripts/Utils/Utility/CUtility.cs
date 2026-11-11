//------------------------------------------------------------
// File : CUtility.cs
// Email: mailto:zhiqiang.yang
// Desc : 基础工具类
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif


public static partial class CUtility
{
    public static string ReplaceFirstStr(string originalString, char targetChar, char replacementChar)
    {
        int _index = originalString.IndexOf(targetChar);
        if (_index != -1)
        {
            string newStr = originalString.Remove(_index, 1).Insert(_index, replacementChar.ToString());
            return newStr;
        }
        return originalString;
    }

    public static class FileUtil
    {
        public static void CreateDirectoryIfNecessary(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || Directory.Exists(dir))
            {
                return;
            }

            Directory.CreateDirectory(dir);
        }

        public static void CreatFile(string filePath, byte[] bytes)
        {
            FileInfo file = new FileInfo(filePath);
            Stream stream = file.Create();

            stream.Write(bytes, 0, bytes.Length);

            stream.Close();
            stream.Dispose();
        }

        public static bool IsFileExist(string path)
        {
            return System.IO.File.Exists(path);
        }
    }


    public static class App
    {
        public static int GetAppVersionCode()
        {
            var versionStr = Application.version.Replace(".", "");
            int.TryParse(versionStr, out int versionCode);
            return versionCode;
        }

        /// <summary>
        /// 是否的电脑editor运行环境
        /// </summary>
        public static bool Editor = Application.platform == RuntimePlatform.OSXEditor ||
                                    Application.platform == RuntimePlatform.WindowsEditor;

        /// <summary>
        /// 是否的安卓运行环境
        /// </summary>
        public static bool Android = Application.platform == RuntimePlatform.Android;

        /// <summary>
        /// 是否是IOS运行环境
        /// </summary>
        public static bool IOS = Application.platform == RuntimePlatform.IPhonePlayer;


        public static float GetScreenMarginTop(float canvasHeight)
        {
            Rect safeArea = Screen.safeArea;
            // Debug.Log("======== GetScreenMarginTop " + safeArea.x + " " + safeArea.y + " " + safeArea.width + " " + safeArea.height + " " + Screen.width + " " + Screen.height);
            return (Screen.height - safeArea.height - safeArea.y) / Screen.height * canvasHeight;
        }

    }

    public static class Mono
    {
        #region Unity功能接口相关
        public static T Instantiate<T>(T t, Transform parent) where T : Component
        {
            if (t == null)
            {
                Log.Debug("Instantiate fail t is null");
                return null;
            }
            //if (parent == null)
            //{
            //    Log.Debug("Instantiate fail parent is null");
            //    return null;
            //}
            T item = UnityEngine.Object.Instantiate<T>(t, Vector3.zero, Quaternion.identity, parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(true);
            return item;
        }

        public static void SetIdentity(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            trans.gameObject.SetActive(true);
        }

        public static void Destroy<T>(T mono) where T : Component
        {
            if (mono == null)
            {
                return;
            }
            Destroy(mono.gameObject);
        }

        public static void Destroy(GameObject go)
        {
            if (go == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
            else
            {
                UnityEngine.Object.Destroy(go);
            }
#else
            UnityEngine.Object.Destroy(go);
#endif
        }


        public static void DestoryCollectionMono<T>(ICollection<T> collect) where T : Component
        {
            if (collect == null)
            {
                return;
            }
            if (collect.Count <= 0)
            {
                return;
            }
            foreach (var item in collect)
            {
                Mono.Destroy(item);
            }
            collect.Clear();
        }

        /// <summary>
        /// 加入一个组件到gameObject里（如果已经存在，则直接返回）
        /// </summary>
        public static T AddComponent<T>(GameObject iObject) where T : Component
        {
            if (iObject == null)
            {
                return null;
            }

            var _Type = iObject.GetComponent<T>();
            if (_Type == null)
                _Type = iObject.AddComponent<T>();
            return _Type;
        }

        /// <summary>
        /// 从gameObject里获取一个Component
        /// </summary>
        public static T GetComponent<T>(GameObject iObject) where T : Component
        {
            if (iObject == null)
            {
                return null;
            }
            return iObject.GetComponent<T>();
        }

        /// <summary>
        /// 从gameObject里获取一个Component
        /// </summary>
        public static T GetComponentInChildren<T>(GameObject iObject) where T : Component
        {
            if (iObject == null)
            {
                return null;
            }
            return iObject.GetComponentInChildren<T>();
        }

        /// <summary>
        /// 获取父节辈的第一组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static T GetParentFirstComponent<T>(GameObject root) where T : Behaviour
        {
            if (root == null)
                return null;
            Transform t = root.transform.parent;
            T canvs = default;
            while (t != null)
            {
                canvs = t.gameObject.GetComponent<T>();
                if (canvs != null)
                    return canvs;
                t = t.parent;
            }
            return null;
        }


        /// <summary>
        /// 排序距离
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collect"></param>
        /// <param name="axis">x:0,y:1,z:2 </param>
        /// <param name="ascending"></param>
        public static void SortTransform<T>(List<T> collect, int axis, bool ascending) where T : Behaviour
        {
            collect.Sort((T a, T b) =>
            {
                float diff = 0;
                if (ascending)
                {
                    if (axis == 0)
                    {
                        diff = a.transform.position.x - b.transform.position.x;
                    }
                    else if (axis == 1)
                    {
                        diff = a.transform.position.y - b.transform.position.y;
                    }
                    else if (axis == 2)
                    {
                        diff = a.transform.position.z - b.transform.position.z;
                    }

                }
                else
                {
                    if (axis == 0)
                    {
                        diff = b.transform.position.x - a.transform.position.x;
                    }
                    else if (axis == 1)
                    {
                        diff = b.transform.position.y - a.transform.position.y;
                    }
                    else if (axis == 2)
                    {
                        diff = b.transform.position.z - a.transform.position.z;
                    }
                }
                if (diff > 0)
                {
                    return 1;
                }
                else if (diff < 0)
                {
                    return -1;
                }
                return 0;
            });
        }

        public static void SortVector3(List<Vector3> collect, int axis, bool ascending)
        {
            collect.Sort((Vector3 a, Vector3 b) =>
            {
                float diff = 0;
                if (ascending)
                {
                    if (axis == 0)
                    {
                        diff = a.x - b.x;
                    }
                    else if (axis == 1)
                    {
                        diff = a.y - b.y;
                    }
                    else if (axis == 2)
                    {
                        diff = a.z - b.z;
                    }

                }
                else
                {
                    if (axis == 0)
                    {
                        diff = b.x - a.x;
                    }
                    else if (axis == 1)
                    {
                        diff = b.y - a.y;
                    }
                    else if (axis == 2)
                    {
                        diff = b.z - a.z;
                    }
                }
                if (diff > 0)
                {
                    return 1;
                }
                else if (diff < 0)
                {
                    return -1;
                }
                return 0;
            });
        }

        /// <summary>
        /// 包装Application.isPlaying，避免在真机中访问
        /// </summary>
        public static bool isApplicationPlaying
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying;
#else
                return true;
#endif
            }
        }
        #endregion

        public static int GetChildActiviteCount(Transform trans)
        {
            int childCount = 0;
            for (int i = 0; i < trans.childCount; i++)
            {
                if (trans.GetChild(i).gameObject.activeInHierarchy) childCount++;
            }
            return childCount;
        }
    }


    public static class Math
    {
        public static int GetDirection(float angle)
        {
            if (angle >= 22.5f && angle < 67.5f)
            {
                return 2;//右上
            }
            else if (angle >= 67.5f && angle < 112.5f)
            {
                return 3;//右
            }
            else if (angle >= 112.5f && angle < 157.5f)
            {
                return 4;//右下
            }
            else if (angle >= 157.5f && angle < 202.5f)
            {
                return 5;//下
            }
            else if (angle >= 202.5f && angle < 247.5f)
            {
                return 6;//左下
            }
            else if (angle >= 247.5f && angle < 292.5f)
            {
                return 7;//左
            }
            else if (angle >= 292.5f && angle < 337.5f)
            {
                return 8;//左上
            }
            else
            {
                return 1;//上
            }
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return angle;
        }

        public static int PowInt(int f, int p)
        {
            int result = 1;
            for (int i = 0; i < p; i++)
            {
                result = result * f;
            }
            return result;
        }

        /// <summary>
        /// 从数组重随机选取一个
        /// </summary>
        /// <param name="pools"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomFromList<T>(IList<T> pools)
        {
            if (pools == null)
            {
                Log.Error("RandomFromList pools is null;");
                return default(T);
            }

            int count = pools.Count;
            if (count <= 0)
            {
                Log.Error("RandomFromList pools is empty;");
                return default(T);
            }

            if (count == 1)
            {
                return pools[0];
            }

            int rnd = UnityEngine.Random.Range(0, count);
            return pools[rnd];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <param name="myList"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShuffleList<T>(IList<T> myList)
        {
            int index = 0;
            T temp = default(T);
            int count = myList.Count;
            for (int i = 0; i < count; i++)
            {
                index = UnityEngine.Random.Range(0, count);
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }

        //生成随机不重复的数组 [min, max]
        public static int[] GetRandomArray(int min, int max)
        {
            int[] array = new int[max - min + 1];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i + min;
            }

            for (int i = 0; i < array.Length; i++)
            {
                int temp = array[i];
                int randomIndex = UnityEngine.Random.Range(0, array.Length);
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }

            return array;
        }

        /// <summary>
        /// 从数组中取 num 个元素返回
        /// </summary>
        /// <param name="myList"></param>
        /// <param name="num"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> RandomChoose<T>(IList<T> myList, int num)
        {
            ShuffleList(myList);

            List<T> list = new List<T>();

            if (myList.Count <= num)
            {
                list.AddRange(myList);
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    list.Add(myList[i]);
                }
            }
            return list;
        }

        public static T Clamp<T>(T v, T min, T max) where T : System.IComparable<T>
        {
            if (v.CompareTo(min) < 0)
                return min;
            else if (v.CompareTo(max) > 0)
                return max;
            return v;
        }


        // 排列（顺序敏感）
        public static List<List<T>> GetPermutations<T>(List<T> elements)
        {
            var result = new List<List<T>>();
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = 0; j < elements.Count; j++)
                {
                    if (i != j)
                    {
                        result.Add(new List<T> { elements[i], elements[j] });
                    }
                }
            }
            return result;
        }

        // 组合（顺序不敏感）
        public static List<List<T>> GetCombinations<T>(List<T> elements)
        {
            var result = new List<List<T>>();
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    result.Add(new List<T> { elements[i], elements[j] });
                }
            }
            return result;
        }

        public static float CompareCloseness(float x, float minX, float maxX)
        {
            float distanceToMin = Mathf.Abs(x - minX);
            float distanceToMax = Mathf.Abs(x - maxX);

            if (Mathf.Approximately(distanceToMin, distanceToMax))
                return minX;
            return (distanceToMin < distanceToMax) ? minX : maxX;
        }
    }


    public static class UGUITools
    {
        public static bool UIInScreen(RectTransform target, Rect area, Transform root, out Vector2 offset)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, target);

            Vector2 delta = default;
            if (bounds.center.x - bounds.extents.x < area.x)//左
            {
                delta.x += Mathf.Abs(bounds.center.x - bounds.extents.x - area.x);
            }
            else if (bounds.center.x + bounds.extents.x > area.width / 2)//右
            {
                delta.x -= Mathf.Abs(bounds.center.x + bounds.extents.x - area.width / 2);
            }

            if (bounds.center.y - bounds.extents.y < area.y)//上
            {
                delta.y += Mathf.Abs(bounds.center.y - bounds.extents.y - area.y);
            }
            else if (bounds.center.y + bounds.extents.y > area.height / 2)//下
            {
                delta.y -= Mathf.Abs(bounds.center.y + bounds.extents.y - area.height / 2);
            }

            offset = delta;
            //target.anchoredPosition += delta;

            return delta == default;
        }
    }


    public static class StringHelper
    {

        // public static readonly string[] romanNum = new string[] { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ" };
        public static readonly string[] romanNum = new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

        public static string ArrayToString<T>(IList<T> array)
        {
            if (array == null || array.Count <= 0)
            {
                return "";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < array.Count; i++)
            {
                sb.AppendFormat(array[i].ToString() + "{0}", ",");
            }

            return sb.ToString();
        }

        public static string RemoveRichSymbol(string richText)
        {
            string plainText = Regex.Replace(richText, "<.*?>", String.Empty);
            return plainText;
        }

        public static string RemoveRichSizeSymbol(string richText)
        {
            string plainText = Regex.Replace(richText, "<size=.*?>", String.Empty);
            return plainText;
        }

        //获取罗马数字
        public static string GetRomanNum(int num)
        {
            if (num > 0 && num <= 10)
                return "<b>" + romanNum[num - 1] + "</b>";
            return "";
        }

    }

    public static class LoadFile
    {
        // 支持多平台的文本文件读取
        public static string GetFileStr(string path)
        {
            string jsonStr = "";
#if UNITY_ANDROID || UNITY_IOS
            jsonStr = GetStreamingPathStr(path);
#else
            jsonStr = FileRead(path);
#endif
            return jsonStr;
        }

        public static string GetStreamingPathStr(string path)
        {
            string fileStr = "";
            var uri = new System.Uri(path);
            var request = UnityWebRequest.Get(uri);

            var www = request.SendWebRequest();
            if (request.isNetworkError || request.isNetworkError)
            {
                // MyLog.log.Debug(request.error);
                Log.Error(request.error);
            }
            else
            {
                while (true)
                {
                    if (!request.isDone) continue;
                    fileStr = request.downloadHandler.text;
                    break;
                }
            }
            return fileStr;
        }

        public static string FileRead(string path)
        {
            StreamReader sr = File.OpenText(path);

            //读取到文件最后
            string str = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return str;
        }

    }

    public static class TimerUtil
    {
        public static long GetCurrentUnixTime()
        {
            DateTime startTime = new DateTime(1970, 1, 1, 8, 0, 0);
            return (long)(DateTime.Now - startTime).TotalSeconds;
        }

        public static string FormatTwoWithUnit(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds - hours * 3600) / 60;
            int seconds = totalSeconds - hours * 3600 - minutes * 60;
            if (hours > 0)
            {
                return string.Format("{0}h {1}m", hours, minutes);
            }
            else
            {
                return string.Format("{0}m {1}s", minutes, seconds);
            }
        }

        public static string FormatSeconds(int totalSeconds, bool forceThree = false)
        {
            if (totalSeconds > 3600 || forceThree)
                return FormatTime(totalSeconds);
            else
                return FormatTwoTime(totalSeconds);
        }

        public static string FormatTwoTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            string mm = minutes < 10f ? "0" + minutes : minutes.ToString();
            int seconds = (totalSeconds - (minutes * 60));
            string ss = seconds < 10 ? "0" + seconds : seconds.ToString();
            return string.Format("{0}:{1}", mm, ss);
        }

        public static string FormatTimeH(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            string hh = hours < 10 ? "0" + hours : hours.ToString();
            return hh;
        }

        public static string FormatTimeM(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            string hh = hours < 10 ? "0" + hours : hours.ToString();
            int minutes = (totalSeconds - hours * 3600) / 60;
            string mm = minutes < 10f ? "0" + minutes : minutes.ToString();
            return mm;
        }

        public static string FormatTimeS(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds - hours * 3600) / 60;
            int seconds = totalSeconds - hours * 3600 - minutes * 60;
            string ss = seconds < 10 ? "0" + seconds : seconds.ToString();
            return ss;
        }

        public static string FormatTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            string hh = hours < 10 ? "0" + hours : hours.ToString();
            int minutes = (totalSeconds - hours * 3600) / 60;
            string mm = minutes < 10f ? "0" + minutes : minutes.ToString();
            int seconds = totalSeconds - hours * 3600 - minutes * 60;
            string ss = seconds < 10 ? "0" + seconds : seconds.ToString();
            return string.Format("{0}:{1}:{2}", hh, mm, ss);
        }

        public static string FormatDayTime(int totalSeconds)
        {
            int days = (totalSeconds / 3600) / 24;
            string dd = days < 10 ? "0" + days : days.ToString();
            int hours = (totalSeconds / 3600) - (days * 24);
            string hh = hours < 10 ? "0" + hours : hours.ToString();
            int minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
            string mm = minutes < 10f ? "0" + minutes : minutes.ToString();
            int seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);
            string ss = seconds < 10 ? "0" + seconds : seconds.ToString();
            return string.Format("{0}:{1}:{2}:{3}", dd, hh, mm, ss);
        }

        public static string GetSuffix(decimal num)
        {
            string suffix = "";
            string s = num.ToString();
            if (num < 1000) return num.ToString();
            suffix = GetSuffix((s.Length - 1) / 3);
            if (s.Length % 3 == 1)
                return s.Substring(0, 1) + "." + s.Substring(1, 1) + s.Substring(2, 1) + suffix; // 4.35m
            if (s.Length % 3 == 2)
                return s.Substring(0, 1) + s.Substring(1, 1) + "." + s.Substring(2, 1) + suffix; // 43.5m
            if (s.Length % 3 == 0)
                return s.Substring(0, 1) + s.Substring(1, 1) + s.Substring(2, 1) + suffix; // 435m
            return "";
        }

        /// <summary>
        /// 格式化货币数量“###,###K”
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FormatCoin(decimal num)
        {
            if (num < 1)
                return "0";
            if (num < 100000000)
            {
                return num.ToString("###,###");
            }
            int power = 0;
            float count = (float)num;
            while (count >= 1000000)
            {
                count /= 1000;
                power++;
            }
            num = decimal.Floor((decimal)count);
            string s = num.ToString();
            string ss = s;
            switch (s.Length % 6)
            {
                case 0:
                    ss = s.Substring(0, 3) + "." + s.Substring(3, 3);
                    break;
                case 1:
                    ss = s + ".000";
                    break;
                case 2:
                    ss = s + ".000";
                    break;
                case 3:
                    ss = s + ".000";
                    break;
                case 4:
                    ss = s.Substring(0, 1) + "." + s.Substring(1, 3);
                    break;
                case 5:
                    ss = s.Substring(0, 2) + "." + s.Substring(2, 3);
                    break;
            }
            power++;
            return ss + GetSuffix(power);
        }

        private static string GetSuffix(int power)
        {
            string suffix = string.Empty;
            switch (power)
            {
                case 0: suffix = string.Empty; break;
                case 1: suffix = "K"; break;
                case 2: suffix = "M"; break;
                case 3: suffix = "B"; break;
                case 4: suffix = "T"; break;
                case 5: suffix = "q"; break;
                case 6: suffix = "Q"; break;
                case 7: suffix = "s"; break;
                case 8: suffix = "S"; break;
                case 9: suffix = "o"; break;
                case 10: suffix = "n"; break;
                case 11: suffix = "d"; break;
                case 12: suffix = "u"; break;
                case 13: suffix = "du"; break;
                case 14: suffix = "tr"; break;
                case 15: suffix = "qu"; break;
                case 16: suffix = "Qu"; break;
                case 17: suffix = "se"; break;
                case 18: suffix = "Se"; break;
                case 19: suffix = "oc"; break;
                case 20: suffix = "no"; break;
                case 21: suffix = "vi"; break;
            }
            return suffix;
        }


        /// <summary>
        /// 检测手指是否在UI上
        /// </summary>
        /// <returns></returns>
        // public static bool IsPointerOverGameObject()
        // {
        //     if (null == EventSystem.current)
        //         return false;
        //     PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        //     {
        //         pressPosition = Input.mousePosition,
        //         position = Input.mousePosition
        //     };
        //     List<RaycastResult> list = new List<RaycastResult>();
        //     UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, list);
        //     return list.Count > 0;
        // }
    }

    public static class Net
    {
        public static bool NetReady
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

    }

    public static class ColorUtils
    {
        public static Color ParseHtmlString(string color)
        {
            Color newColor;
            ColorUtility.TryParseHtmlString(color.Trim(), out newColor);
            return newColor;
        }
    }


    public static class Tools
    {
        public static List<int> SplitStringToInt(string source, params char[] separator)
        {
            List<int> results = new List<int>();

            try
            {
                var elements = source.Split(separator);
                for (int i = 0; i < elements.Length; i++)
                {
                    results.Add(int.Parse(elements[i]));
                }
            }
            catch (System.Exception)
            {
                UnityGameFramework.Runtime.Log.Error("SplitStringToInt parse fail source: ", source);
            }

            return results;
        }

        public static string ToString(byte[] array)
        {
            if ((array == null) || (array.Length <= 0))
            {
                return "";
            }
            return Encoding.UTF8.GetString(array, 0, array.Length);

            //        int size = 0;
            //        foreach (byte item in array)
            //        {
            //            if (item == 0)
            //                break;
            //
            //            ++size;
            //        }
            //
            //        return Encoding.UTF8.GetString(array, 0, size);
        }

        public static (Func<float> get, Action<float> set) AlphaGetSet(Component monoBehaviour)
        {
            if (monoBehaviour is CanvasGroup canvasGroup)
            {
                return (() => { return canvasGroup.alpha; }, (a) => { canvasGroup.alpha = a; });
            }
            else if (monoBehaviour is UnityEngine.UI.Image image)
            {
                return (() => { return image.GetAlpha(); }, (a) => { image.SetAlpha(a); });
            }
            else if (monoBehaviour is SpriteRenderer spriteRenderer)
            {
                return (() => { return spriteRenderer.GetAlpha(); }, (a) => { spriteRenderer.SetAlpha(a); });
            }
            else if (monoBehaviour is TMPro.TextMeshProUGUI textMeshProUGUI)
            {
                return (() => { return textMeshProUGUI.alpha; }, (a) => { textMeshProUGUI.alpha = a; });
            }

            return default;
        }
    }

    public static long GetLong(string settingName, long defaultValue)
    {
        string strValue = PlayerPrefs.GetString(settingName, defaultValue.ToString());
        long value = 0;
        if (long.TryParse(strValue, out value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static void SetLong(string settingName, long value)
    {
        string strValue = value.ToString();
        PlayerPrefs.SetString(settingName, strValue);
    }

    public static void SetAlpha(this Image image, float i)
    {
        var color = image.color;
        var res = Mathf.Clamp(i, 0, 1);
        color.a = res;
        image.color = color;
    }

    public static void SetAlpha(this SpriteRenderer image, float i)
    {
        var color = image.color;
        var res = Mathf.Clamp(i, 0, 1);
        color.a = res;
        image.color = color;
    }

    public static float GetAlpha(this SpriteRenderer image)
    {
        return image.color.a;
    }

    public static float GetAlpha(this Image image)
    {
        return image.color.a;
    }

    public static float GetAspectRatio(this Sprite sprite, bool isWidth)
    {
        if (isWidth)
        {
            return sprite.rect.height / sprite.rect.width;
        }
        else
        {
            return sprite.rect.width / sprite.rect.height;
        }
    }
}