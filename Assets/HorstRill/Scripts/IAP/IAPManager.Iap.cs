using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IapRequest
{
    public string receiptBase64;
}

[Serializable]
public class IapRequestAndroid
{
    public string productId;
    public string purchaseToken;
    public string type;
}

[Serializable]
public class IapReceipt
{
    public string transactionId;
    public string productId;
    public string type;

    public IapReceipt(string _productId, string _transactionID)
    {
        productId = _productId;
        transactionId = _transactionID;
    }
}
