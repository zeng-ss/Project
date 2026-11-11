using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace CasualGame
{
    public static class ItemUtils
    {
        /// <summary>
        /// 获取Item的数量文本（一般展示于物品下方）
        /// </summary>
        public static string GetNumByStyle(DRItem item, int num)
        {
            if (item == null) return num.ToString();

            ItemShowStyle style = item.GetItemShowStyle();
            if (style == ItemShowStyle.Add)
            {
                return string.Format("x{0}", num);
            }
            else if (style == ItemShowStyle.Mult)
            {
                return string.Format("x{0}", num);
            }
            else if (style == ItemShowStyle.Time)
            {
                return GetDurationTxt(num);
            }
            else if (style == ItemShowStyle.Empty)
            {
                return num.ToString();
            }
            return "";
        }

        public static string GetMultNumByStyle(DRItem item, int num)
        {
            ItemShowStyle style = item.GetItemShowStyle();
            if (style == ItemShowStyle.Time)
            {
                return GetDurationTxt(num);
            }
            else if (style == ItemShowStyle.Add)
            {
                return string.Format("x{0}", num);
            }
            else
            {
                return string.Format("x{0}", num);
            }
        }

        /// <summary>
        /// 获取时间长度
        /// </summary>
        public static string GetDurationTxt(int num)
        {
            var durationMin = num / 60;
            int hour = durationMin / 60;
            int min = durationMin % 60;
            if (hour > 0)
            {
                if (min > 0)
                {
                    return $"{hour}h{min}m";
                }
                else
                {
                    return $"{hour}h";
                }
            }
            else
            {
                return $"{min}m";
            }
        }

        public static List<ItemData> GetItems(string rewardStr, bool removeRepeat = false)
        {
            List<ItemData> retList = new List<ItemData>();

            if (rewardStr == "-1" || rewardStr == "-1.0" || string.IsNullOrEmpty(rewardStr)) //空数据
            {
                return retList;
            }

            try
            {
                string[] rewards = rewardStr.Split(';');

                foreach (var reward in rewards)
                {
                    string[] rewardItem = reward.Split(',');

                    ItemData item = new ItemData(int.Parse(rewardItem[0]), int.Parse(rewardItem[1]));
                    retList.Add(item);
                }

                if (removeRepeat)
                {
                    return GetItemsByFilter(retList);
                }
                else
                {
                    return retList;
                }
            }
            catch (Exception ex)
            {

                UnityGameFramework.Runtime.Log.Error("解析异常 ex:{0}   reward:{1}", ex.Message, rewardStr);

                return retList;
            }

        }

        /// <summary>
        /// 过滤重复奖励
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<ItemData> GetItemsByFilter(List<ItemData> items)
        {
            Dictionary<int, ItemData> allItems = new Dictionary<int, ItemData>();
            foreach (var item in items)
            {
                if (!allItems.ContainsKey(item.id))
                {
                    allItems.Add(item.id, item);
                }
                else
                {
                    allItems[item.id].num += item.num;
                }
            }
            return allItems.Values.ToList();
        }
    }

}