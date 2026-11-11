using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Newtonsoft.Json;
using CasualGame;
using PuzzleTrack;

public enum IAPSourceType
{
    none,
    shop,
    dialog,
}

public enum VipType
{
    None,
    Week,
    Month,
    Year
}

public partial class IAPManager
{
    private static IAPManager _instance;
    public static IAPManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new IAPManager();
            return _instance;
        }
    }

    private StoreController m_StoreController;
    Action<string> IapSuccessAction;
    Action<string, string> IapFailAction;
    IAPSourceType IapSourceType = IAPSourceType.none;
    DRItemIAP IapItem;
    private bool hasCheckSubscribeReceiptLocal = false;
    Dictionary<string, ProductMetadata> m_LocalPrizes = new Dictionary<string, ProductMetadata>();
    public bool SkipPurchase = false;
    private bool _isIAPInitialized = false;
    private bool _eventsSubscribed;
    private bool _isConnecting;
    public bool IsIAPInitialized => _isIAPInitialized;

    public bool IsInitialized()
    {
        return m_StoreController != null && _isIAPInitialized;
    }

    public void Initilize()
    {
        _isIAPInitialized = false;
        if (m_StoreController == null)
        {
            hasCheckSubscribeReceiptLocal = false;
            InitializePurchasingAsync().Forget();
            LogHandler.Info("==> lyly IAP - Initilize");
        }
        SkipPurchase = false;
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        if (m_StoreController == null)
            InitializePurchasingAsync().Forget();
    }

    private async UniTaskVoid InitializePurchasingAsync()
    {
        if (_isConnecting || m_StoreController != null)
            return;

        _isConnecting = true;
        try
        {
            m_StoreController = UnityIAPServices.StoreController();
            SubscribeStoreEvents();
            await m_StoreController.Connect();
        }
        catch (Exception e)
        {
            LogHandler.Info($"IAP - Connect failed: {e.Message}");
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private void SubscribeStoreEvents()
    {
        if (_eventsSubscribed || m_StoreController == null)
            return;

        _eventsSubscribed = true;

        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
        m_StoreController.OnPurchaseDeferred += OnPurchaseDeferred;
        m_StoreController.OnStoreConnected += OnStoreConnected;
        m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
        m_StoreController.OnProductsFetched += OnProductsFetched;
        m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;
        m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
        m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
    }

    private void OnStoreConnected()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_StoreController.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(HorstRillSdk.GetDeviceId());
#endif
        LogHandler.Info("==> lyly IAP - OnStoreConnected");
        m_StoreController.FetchProducts(BuildProductDefinitions());
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        _isIAPInitialized = false;
        LogHandler.Info($"IAP - OnStoreDisconnected: {failure?.Message}");
    }

    private void OnProductsFetched(List<Product> products)
    {
        InitLocalPrizes();
        m_StoreController.FetchPurchases();
        _isIAPInitialized = true;
        LogHandler.Info("IAP - OnProductsFetched: PASS");
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        LogHandler.Info($"IAP - OnProductsFetchFailed: {failure?.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        if (!hasCheckSubscribeReceiptLocal)
        {
            bool checkVip = ProxyManager.UserProxy != null && ProxyManager.UserProxy.userModel != null;
            if (checkVip)
                CheckSubscribeReceipt();
            else
                ConditionEventManager.Instance.EnqueueAction(CheckSubscribeReceipt, () => ProxyManager.UserProxy != null && ProxyManager.UserProxy.userModel != null);
        }

        LogHandler.Info("==> lyly IAP - OnPurchasesFetched");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        LogHandler.Info($"IAP - OnPurchasesFetchFailed: {failure?.Message}, {failure?.FailureReason}");
    }

    private void OnPurchaseDeferred(DeferredOrder deferredOrder)
    {
        UIManager.CloseLoading();
        LogHandler.Info("IAP - OnPurchaseDeferred");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case ConfirmedOrder confirmedOrder:
                LogHandler.Info($"IAP - OnPurchaseConfirmed: {GetProductFromOrder(confirmedOrder)?.definition.id}");
                break;
            case FailedOrder failedOrder:
                LogHandler.Info($"IAP - OnPurchaseConfirmed failed: {failedOrder.FailureReason}, {failedOrder.Details}");
                break;
        }
    }

    private List<ProductDefinition> BuildProductDefinitions()
    {
        var productDefinitions = new List<ProductDefinition>();
        var filterIds = new HashSet<string>();
        GameFramework.DataTable.IDataTable<DRItemIAP> iaps = GameEntry.DataTable.GetDataTable<DRItemIAP>();
        foreach (var item in iaps)
        {
            if (filterIds.Contains(item.BaseProductId))
                continue;

#if UNITY_IOS
            if (item.Platform == 2 || item.Platform == 0)
            {
#elif UNITY_ANDROID
            if (item.Platform == 1 || item.Platform == 0)
            {
#endif
#if UNITY_IOS || UNITY_ANDROID
                var productType = item.ProductType == 0 ? ProductType.Consumable : ProductType.Subscription;
                productDefinitions.Add(new ProductDefinition(item.BaseProductId, productType));
                filterIds.Add(item.BaseProductId);
            }
#endif
        }

        LogHandler.Info("==> lyly IAP - BuildProductDefinitions");
        return productDefinitions;
    }

    private void InitLocalPrizes()
    {
        m_LocalPrizes.Clear();

        GameFramework.DataTable.IDataTable<DRItemIAP> iaps = GameEntry.DataTable.GetDataTable<DRItemIAP>();
        foreach (var item in iaps)
        {
#if UNITY_IOS
            if (item.Platform == 2 || item.Platform == 0)
            {
                var product = GetProduct(item.BaseProductId);
                if (product != null && !m_LocalPrizes.ContainsKey(item.ProductId))
                    m_LocalPrizes.Add(item.ProductId, product.metadata);
            }
#elif UNITY_ANDROID
            if (item.Platform == 1 || item.Platform == 0)
            {
                var product = GetProduct(item.BaseProductId);
                if (product != null && !m_LocalPrizes.ContainsKey(item.ProductId))
                    m_LocalPrizes.Add(item.ProductId, product.metadata);
            }
#endif
        }
        LogHandler.Info("==> lyly IAP - InitLocalPrizes");
    }

    private Product GetProduct(string productId)
    {
        return m_StoreController?.GetProductById(productId);
    }

    private static Product GetProductFromOrder(Order order)
    {
        return order?.CartOrdered?.Items()?.FirstOrDefault()?.Product;
    }

    public void BuyProductID(DRItemIAP item, IAPSourceType sourceType, Action<string> _iapSuccessAction = null, Action<string, string> _iapFailAction = null)
    {
        if (item == null)
        {
            Debug.LogError("==> lyly IAP - BuyProductID : item is null");
            return;
        }
        LogHandler.Info("==> lyly IAP - BuyProductID Item: " + item.ProductId);

#if UNITY_EDITOR
        SkipPurchase = true;
#endif

        if (SkipPurchase)
        {
            UIManager.ShowLoading(true);
            CoroutineHelper.Instance.AttachTimer(1f, () =>
            {
                UIManager.CloseLoading();
                _iapSuccessAction?.Invoke(item.ProductId);
                PuzzleTracking.Iap.PaymentSuccess(
                    "test",
                    item.PrizeValue,
                    item.PrizeValue,
                    item.ProductId,
                    item.ProductId,
                    isFirstPay(),
                    1,
                    sourceType.ToString(),
                    $"test_{GameFramework.Utility.Random.GetRandom(1, 101)}_{GameFramework.Utility.Random.GetRandom(1, 101)}",
                    "");
                PuzzleTracking.Iap.SPackPurchase(item.EventName, item.PrizeValue);
                ProxyManager.UserProxy.AddTransactionID($"transaction_{GameFramework.Utility.Random.GetRandom(1, 101)}_id_{GameFramework.Utility.Random.GetRandom(1, 101)}", item.ProductId);
            });
            return;
        }

        if (IsInitialized())
        {
            var productId = item.BaseProductId;
            var productIdReal = item.ProductId;
            IapItem = item;
            IapSuccessAction = _iapSuccessAction;
            IapFailAction = _iapFailAction;
            IapSourceType = sourceType;
            LogHandler.Info($"IAP - BuyProductID : {productId} - {productIdReal}");

            Product product = GetProduct(productId);
            if (product != null)
            {
                ProductMetadata metadata = product.metadata;
                PuzzleTracking.Iap.PaymentInit(
                    metadata.isoCurrencyCode,
                    (float)metadata.localizedPrice,
                    IapItem.PrizeValue,
                    metadata.localizedTitle,
                    productIdReal,
                    sourceType.ToString(),
                    productId);
            }

            if (product != null && product.availableToPurchase)
            {
                UIManager.ShowLoading(true);
                LogHandler.Info($"IAP - BuyProductID - PurchaseProduct : {product.definition.id} - {productIdReal}");
                m_StoreController.PurchaseProduct(product);
            }
            else
            {
                IapFailAction?.Invoke(productIdReal, "Not purchasing product, either is not found or is not available for purchase");
                LogHandler.Info("IAP - BuyProductID : FAIL. Not purchasing product, either is not found or is not available for purchase");
                UIManager.Toast(LocalizationUtils.GetString(45, "Purchase failed, please try again."));
            }
        }
        else
        {
            IapFailAction?.Invoke(item.ProductId, "Initiate Failed. Please check the IAP configuration.");
            Debug.LogError("Initiate Failed. Please check the IAP configuration.");
        }
    }

    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        var product = GetProductFromOrder(pendingOrder);
        if (product == null)
        {
            UIManager.CloseLoading();
            LogHandler.Info("IAP - OnPurchasePending: product is null");
            return;
        }

        LogHandler.Info($"IAP - OnPurchasePending: PASS. Trigger: {product.definition.storeSpecificId}, {pendingOrder.Info.TransactionID}");
        PuzzleTracking.Iap.PaymentStateCheck(
            PuzzleIapTracking.SdkSuccess,
            IapItem != null ? IapItem.ProductId : product.definition.id,
            pendingOrder.Info.TransactionID,
            PuzzleIapTracking.Standrand,
            product.definition.id);

        CheckStorePaymentValid(pendingOrder, product, needConfirm =>
        {
            _OnPurchaseSuccess(pendingOrder, product, needConfirm);
        }, showTips =>
        {
            UIManager.CloseLoading();
            if (showTips)
                UIManager.Toast(LocalizationUtils.GetString(46, "Invalid Payment"));
            IapSuccessAction = null;
            IapFailAction = null;
        });
    }

    private void CheckStorePaymentValid(Order order, Product product, Action<bool> success, Action<bool> fail)
    {
        string transactionId = order.Info.TransactionID;
        bool hasTransactionID = ProxyManager.UserProxy.HasTransactionID(transactionId);
        if (hasTransactionID)
        {
            LogHandler.Info($"IAP - Skip record : {transactionId}");
            fail?.Invoke(false);
            PuzzleTracking.Iap.TransactionCheck(transactionId, 1);
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        success?.Invoke(true);
        return;
#endif

        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);

        try
        {
            var result = validator.Validate(order.Info.Receipt);

            for (int i = 0; i < result.Length; i++)
            {
                IPurchaseReceipt productReceipt = result[i];

                if (!string.Equals(productReceipt.productID, product.definition.id))
                    continue;

                GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                if (google != null)
                {
                    LogHandler.Info("IAP - [Receipt] Android transactionID:" + google.transactionID);
                    LogHandler.Info("IAP - [Receipt] Android purchaseState:" + google.purchaseState);
                    LogHandler.Info("IAP - [Receipt] Android purchaseToken:" + google.purchaseToken);
                    success?.Invoke(true);
                    return;
                }
            }

            fail?.Invoke(true);
        }
        catch (IAPSecurityException e)
        {
            LogHandler.Info("IAP - Invalid receipt");
            fail?.Invoke(true);
            PuzzleTracking.Iap.TransactionCheck(transactionId, 2, e.Message);
        }
    }

    private void _OnPurchaseSuccess(PendingOrder pendingOrder, Product product, bool needConfirm)
    {
        LogHandler.Info($"IAP - OnPurchaseSuccess: PASS. Product: '{product.definition.storeSpecificId}', {pendingOrder.Info.TransactionID}, IapItem.BaseProductId = {IapItem?.BaseProductId}");
        UIManager.CloseLoading();

        if (IapSuccessAction != null && IapItem != null && product.definition.id.Equals(IapItem.BaseProductId))
        {
            IapSuccessAction.Invoke(IapItem.ProductId);
            LogHandler.Info("IAP - 发奖!!!");
            ProxyManager.UserProxy.AddTransactionID(pendingOrder.Info.TransactionID, product.definition.id);

            if (needConfirm)
                m_StoreController.ConfirmPurchase(pendingOrder);

            ProductMetadata metadata = product.metadata;
            bool is_first_pay = isFirstPay();
            PuzzleTracking.Iap.PaymentSuccess(
                metadata.isoCurrencyCode,
                (float)metadata.localizedPrice,
                IapItem.PrizeValue,
                metadata.localizedTitle,
                IapItem.ProductId,
                is_first_pay,
                1,
                IapSourceType.ToString(),
                pendingOrder.Info.TransactionID,
                product.definition.id);
            PuzzleTracking.Iap.SPackPurchase(IapItem.EventName, IapItem.PrizeValue);
        }
        else
        {
            Debug.LogError("IAP - 发奖失败!!!");
            if (needConfirm)
                m_StoreController.ConfirmPurchase(pendingOrder);
        }

        IapSuccessAction = null;
        IapFailAction = null;
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        var product = GetProductFromOrder(failedOrder);
        if (product == null)
        {
            UIManager.CloseLoading();
            LogHandler.Info("IAP - OnPurchaseFailed: product is null");
            return;
        }

        if (Application.platform == RuntimePlatform.Android
            && failedOrder.FailureReason == PurchaseFailureReason.DuplicateTransaction)
        {
            CheckStorePaymentValid(failedOrder, product, needConfirm =>
            {
                _OnPurchaseSuccess(new PendingOrder(failedOrder.CartOrdered, failedOrder.Info), product, needConfirm);
            }, showTips =>
            {
                _OnPurchaseFailed(product, failedOrder.FailureReason, failedOrder.Info.Receipt);
            });
            return;
        }

        _OnPurchaseFailed(product, failedOrder.FailureReason, failedOrder.Info.Receipt);
    }

    private void _OnPurchaseFailed(Product product, PurchaseFailureReason failureReason, string receipt)
    {
        UIManager.CloseLoading();

        if (failureReason != PurchaseFailureReason.DuplicateTransaction)
            UIManager.Toast(LocalizationUtils.GetString(47, "Purchase failed, please try again."));

        ProductMetadata metadata = product.metadata;
        IapFailAction?.Invoke(IapItem != null ? IapItem.ProductId : product.definition.id, failureReason.ToString());
        IapSuccessAction = null;
        IapFailAction = null;

        LogHandler.Info($"IAP - OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
        PuzzleTracking.Iap.PaymentFail(
            metadata.isoCurrencyCode,
            (float)metadata.localizedPrice,
            metadata.localizedTitle,
            IapItem != null ? IapItem.ProductId : product.definition.id,
            receipt,
            IapSourceType.ToString(),
            failureReason.ToString(),
            product.definition.id);
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            LogHandler.Info("IAP - RestorePurchases FAIL. Not initialized.");
            UIManager.Toast(LocalizationUtils.GetString(1379, "There is no purchase record available to restore right now."));
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer
            || Application.platform == RuntimePlatform.OSXPlayer)
        {
            m_StoreController.RestoreTransactions((result, str) =>
            {
                LogHandler.Info(result ? "Restore successful." : $"Restore failed: {str}");
            });
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            m_StoreController.FetchPurchases();
        }
        else
        {
            LogHandler.Info("IAP - RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void CheckSubscribeReceipt()
    {
        if (!IsInitialized())
            return;

        bool isVip = _CheckSubscribeReceipt();
        if (!isVip)
            ProxyManager.UserProxy.ClearVIP();
        LogHandler.Info("==> lyly IAP - CheckSubscribeReceipt isVip: " + isVip);
        PuzzleTracking.Iap.SubscribeStateCheck(PuzzleIapTracking.Sdk, isVip);
        hasCheckSubscribeReceiptLocal = true;
    }

    public bool _CheckSubscribeReceipt()
    {
        bool isSubscribe = false;
        if (m_StoreController == null)
            return false;

        foreach (Order order in m_StoreController.GetPurchases())
        {
            if (order is not ConfirmedOrder confirmedOrder)
                continue;

            var purchasedInfoList = confirmedOrder.Info.PurchasedProductInfo;
            if (purchasedInfoList == null)
                continue;

            foreach (var purchasedInfo in purchasedInfoList)
            {
                SubscriptionInfo subscriptionInfo = purchasedInfo.subscriptionInfo;
                if (subscriptionInfo == null)
                    continue;

                LogHandler.Info("==> lyly IAP - 产品:" + purchasedInfo.productId);
                LogHandler.Info("==> lyly IAP - 是否为可自动续订的商品:" + subscriptionInfo.IsAutoRenewing());
                LogHandler.Info("==> lyly IAP - 是否过期:" + subscriptionInfo.IsExpired());
                LogHandler.Info("==> lyly IAP - 已取消此商品。取消的订阅意味着商品目前为已订阅状态，但不会在下一个账单日期续订:" + subscriptionInfo.IsCancelled());
                LogHandler.Info("==> lyly IAP - 下次自动续订或到期（如果订阅已取消自动续订）的日期" + subscriptionInfo.GetExpireDate());

                if (subscriptionInfo.IsExpired() == Result.False)
                {
                    isSubscribe = true;
                    AddVIPDay(subscriptionInfo);
                }
            }
        }

        return isSubscribe;
    }

    private void AddVIPDay(SubscriptionInfo subscriptionInfo)
    {
        var period = subscriptionInfo.GetSubscriptionPeriod();
        ProxyManager.UserProxy.RecordVipType(period);
        DateTime expirationDate = subscriptionInfo.GetExpireDate();
        TimeSpan remainingTime = expirationDate - DateTime.UtcNow;
        ProxyManager.UserProxy.RecordVipRemainTime(remainingTime);
        LogHandler.Info($"==> lyly IAP - expirationDate: {expirationDate}, 剩余时间: {remainingTime}");
    }

    private bool isFirstPay()
    {
        bool is_first_pay = GameEntry.Setting.GetBool("is_first_pay", true);
        if (is_first_pay)
            GameEntry.Setting.SetBool("is_first_pay", false);
        return is_first_pay;
    }

    public ProductMetadata GetProductMetadata(string productId)
    {
        if (m_LocalPrizes.ContainsKey(productId))
            return m_LocalPrizes[productId];
        return null;
    }

    public void OnRevPurchasing(string productID)
    {
    }

    public void OnRevPurchasedProduct(string productID)
    {
    }

    public void OnRevRestoredProduct(string productID)
    {
        var iap = StoreUtils.GetItemIAP(productID);
        if (iap == null) return;
    }

    public void OnRevAppReceipt(string receipt)
    {
        if (string.IsNullOrEmpty(receipt)) return;
    }

    public void OnRevAppReceiptWhenAppStart(string receipt)
    {
    }
}
