//------------------------------------------------------------
// File : DRExcelExtend.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

namespace CasualGame
{
    public enum ItemShowStyle
    {
        Add = 1,  // +100
        Mult = 2, // x100
        Time = 3, // 2h3min
        Empty = 4,
    }

    public static class DRExcelExtend
    {

        public static bool IsTableEmpty(string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            if (str == "-1" || str == "-1.0") return true;
            return false;
        }
        public static ItemShowStyle GetItemShowStyle(this DRItem item)
        {
            return (ItemShowStyle)item.Style;
        }

        public static string GetPrize(this DRItemIAP iap)
        {
#if !UNITY_EDITOR
            var product = IAPManager.Instance.GetProductMetadata(iap.ProductId);
            if (product != null && !string.IsNullOrEmpty(product.localizedPriceString))
            {
                // string localizedPriceStrWithoutAlphabet = System.Text.RegularExpressions.Regex.Replace(product.localizedPriceString, "[a-zA-Z]", "");
                // return string.Format("{0} {1}", product.isoCurrencyCode, localizedPriceStrWithoutAlphabet);
                return product.localizedPriceString;
            }
#endif
#if UNITY_EDITOR
            return GameFramework.Utility.Text.Format("${0}", iap.PrizeValue);
#endif
            return "-";
        }

        public static string GetOriginalPrize(this DRItemIAP iap)
        {
#if !UNITY_EDITOR
            var product = IAPManager.Instance.GetProductMetadata(iap.ProductId);
            if (product != null && !string.IsNullOrEmpty(product.localizedPriceString))
            {
                Match match2 = Regex.Match(product.localizedPriceString, @"[A-Za-z]+");
                string symbol = string.Empty;
                if (match2.Success)
                {
                    symbol = match2.Value;
                }

                float localPrice = (float)product.localizedPrice;
                localPrice *= iap.OriginalPrize / iap.PrizeValue;
                string currencySymbolsPattern = @"[\p{Sc}]";
                Match match = Regex.Match(product.localizedPriceString, currencySymbolsPattern);
                if (match.Success)
                {
                    return $"{symbol}{match.Value}{localPrice.ToString("F2")}";
                }
                else
                {
                    return $"{symbol}{localPrice.ToString("F2")}";
                }
            }
#endif
#if UNITY_EDITOR
            return GameFramework.Utility.Text.Format("${0}", iap.OriginalPrize);
#endif
            return "-";
        }

        public static string GetPrizeWithRate(this DRItemIAP iap, float rate)
        {
#if !UNITY_EDITOR
            var product = IAPManager.Instance.GetProductMetadata(iap.ProductId);
            if (product != null && !string.IsNullOrEmpty(product.localizedPriceString))
            {
                 Match match2 = Regex.Match(product.localizedPriceString, @"[A-Za-z]+");
                string symbol = string.Empty;
                if (match2.Success)
                {
                    symbol = match2.Value;
                }

                float localPrice = (float)product.localizedPrice;
                localPrice /= rate;
                string currencySymbolsPattern = @"[\p{Sc}]";
                Match match = Regex.Match(product.localizedPriceString, currencySymbolsPattern);
                if (match.Success)
                {
                    return $"{symbol}{match.Value}{localPrice.ToString("F2")}";
                }
                else
                {
                    return  $"{symbol}{localPrice.ToString("F2")}";
                }
            }
#endif

#if UNITY_EDITOR
            return GameFramework.Utility.Text.Format("${0}", (iap.PrizeValue / rate).ToString("F2"));
#endif
            return "-";
        }
    }
}


