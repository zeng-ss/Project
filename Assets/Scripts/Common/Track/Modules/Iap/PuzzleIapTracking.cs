using System;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;

namespace PuzzleTrack
{
    public sealed class PuzzleIapTracking : PuzzleTrackingModule
    {
        public const string SdkSuccess = "sdk_success";
        public const string Standrand = "standrand";
        public const string Sdk = "sdk";
        public const string RevenueCurrencyUsd = "USD";
        public override string ModuleName => "iap";

        public void PaymentInit(
            string currencyType,
            float payAmount,
            float paymentPriceUsd,
            string paymentName,
            string paymentId,
            string paySource,
            string paymentIdVs = "")
        {
            Track(PuzzleTrackingEvent.PaymentInit, new Dictionary<string, object>
            {
                ["currency_type"] = Safe(currencyType),
                ["pay_amount"] = payAmount,
                ["payment_price_usd"] = paymentPriceUsd,
                ["payment_name"] = Safe(paymentName),
                ["payment_id"] = Safe(paymentId),
                ["payment_id_vs"] = Safe(paymentIdVs),
                ["pay_source"] = Safe(paySource)
            });
        }

        public void PaymentFail(
            string currencyType,
            float payAmount,
            string paymentName,
            string paymentId,
            string token,
            string paySource,
            string failReason,
            string paymentIdVs = "")
        {
            Track(PuzzleTrackingEvent.PaymentFail, new Dictionary<string, object>
            {
                ["currency_type"] = Safe(currencyType),
                ["pay_amount"] = payAmount,
                ["payment_name"] = Safe(paymentName),
                ["payment_id"] = Safe(paymentId),
                ["payment_id_vs"] = Safe(paymentIdVs),
                ["pay_source"] = Safe(paySource),
                ["fail_reason"] = Safe(failReason)
            });
        }

        public void PaymentStateCheck(
            string state,
            string paymentId,
            string transactionId,
            string userType,
            string paymentIdVs = "")
        {
            Track(PuzzleTrackingEvent.PaymentStateCheck, new Dictionary<string, object>
            {
                ["payment_state"] = Safe(state),
                ["payment_id"] = Safe(paymentId),
                ["payment_id_vs"] = Safe(paymentIdVs),
                ["payment_transaction_id"] = Safe(transactionId),
                ["user_purchase_type"] = Safe(userType)
            });
        }

        public void PaymentSuccess(
            string currencyType,
            float payAmount,
            float paymentPriceUsd,
            string paymentName,
            string paymentId,
            bool isFirstPay,
            int payType,
            string paySource,
            string transactionId,
            string paymentIdVs = "")
        {
            string safeCurrencyType = Safe(currencyType);

            RecordPaymentSuccess(payAmount, paymentPriceUsd, safeCurrencyType);

            TrackRevenue(PuzzleTrackingEvent.PaymentSuccess, new Dictionary<string, object>
            {
                ["currency_type"] = safeCurrencyType,
                ["pay_amount"] = payAmount,
                ["payment_price_usd"] = paymentPriceUsd,
                ["payment_name"] = Safe(paymentName),
                ["payment_id"] = Safe(paymentId),
                ["payment_id_vs"] = Safe(paymentIdVs),
                ["is_first_pay"] = isFirstPay,
                ["pay_type"] = payType,
                ["pay_source"] = Safe(paySource),
                ["payment_transaction_id"] = Safe(transactionId)
            }, safeCurrencyType, payAmount);
        }

        public void SubscribeStateCheck(string type, bool state)
        {
            Track(PuzzleTrackingEvent.SubscribeStateCheck, new Dictionary<string, object>
            {
                ["vip_check_type"] = Safe(type),
                ["vip_state"] = state
            });
        }

        public void TransactionCheck(string transactionId, int failType, string message = "")
        {
            Track(PuzzleTrackingEvent.TransactionCheck, new Dictionary<string, object>
            {
                ["transaction_id"] = Safe(transactionId),
                ["fail_type"] = failType,
                ["message"] = Safe(message)
            });
        }

        public void SPackPurchase(string pack, float value)
        {
            if (string.IsNullOrEmpty(pack))
                return;

            TrackRevenue(pack, new Dictionary<string, object>
            {
                ["value"] = value
            }, RevenueCurrencyUsd, value);
        }

        private void RecordPaymentSuccess(float payAmount, float paymentPriceUsd, string currencyType)
        {
            try
            {
                ProxyManager.UserProxy?.AddPayment(payAmount, paymentPriceUsd, currencyType);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PuzzleTracking] payment success recorder failed: {e.Message}");
            }
        }
    }
}
