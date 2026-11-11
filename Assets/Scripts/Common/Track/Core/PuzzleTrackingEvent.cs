namespace PuzzleTrack
{
    internal static class PuzzleTrackingEvent
    {
        public const string LevelTrack = "ai_puzzle_level";
        public const string LevelRecord = "level_record";
        public const string HardCurrency = "ai_puzzle_dl";
        public const string SoftCurrency = "puzzle_dl";
        public const string BoosterTrack = "ai_puzzle_tutorial";
        public const string UITrack = "home_page_show";
        public const string AdOpportunityMissing = "dk_ad_fail";
        public const string ApUaAdsPrefix = "ap_ua_ads_";
        public const string NewPuzzleAdsImpression = "new_puzzle_ads_impression";
        public const string AfConversionData = "af_conversion_data";
        public const string PaymentInit = "payment_init";
        public const string PaymentFail = "payment_fail";
        public const string PaymentStateCheck = "payment_state_check";
        public const string PaymentSuccess = "payment_success";
        public const string SubscribeStateCheck = "subscribe_state_check";
        public const string TransactionCheck = "transaction_check";
        public const string NotificationOpen = "notification_open";
        public const string UnityException = "unity_exception";
        public const string UnityError = "unity_error";
        public const string UnityTryCache = "unity_trycache";
    }
}
