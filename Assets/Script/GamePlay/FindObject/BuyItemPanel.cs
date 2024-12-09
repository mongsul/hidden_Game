using System;
using System.Collections;
using System.Collections.Generic;
using Core.Library;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.Events;

public class BuyItemPanel : MonoBehaviour
{
    [SerializeField] private LocalizeTextField nameField;
    [SerializeField] private LocalizeTextField priceField;
    [SerializeField] private SoundPlayer displaySound;
    
    [Serializable]
    public class BuyEvent : UnityEvent<ProductTable>{}

    private ProductTable product;
    private BuyEvent successEvent = new BuyEvent();
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetPurchaseItem(int productIndex, UnityAction<ProductTable> successCallback)
    {
        product = ShopManager.Instance.GetProductTable(productIndex);
        if (product == null)
        {
            return;
        }

        if (displaySound)
        {
            displaySound.PlaySound();
        }
        
        gameObject.SetActive(true);
        successEvent.RemoveAllListeners();
        if (successCallback != null)
        {
            successEvent.AddListener(successCallback);
        }

        if (nameField)
        {
            LocalizeTextField.LocalizeInfo localizeInfo = new LocalizeTextField.LocalizeInfo();
            localizeInfo.localizeKey = product.productName;
            localizeInfo.SetContent(product.itemValue.ToString());
            nameField.SetText(localizeInfo);
        }

        if (priceField)
        {
            priceField.SetText($"${product.itemCost}");
        }
    }
    
    public void OnClickBuy()
    {
        if (!ShopManager.Instance.IsPossiblePurchase(product))
        {
            MessageManager.Instance.SetMsg("RemoveAdPurchased");
            return;
        }
        
        ShopManager.Instance.RequestBuyProduct(product.productIdx, OnPurchaseItem);
    }

    private void OnPurchaseItem(PurchaseError error, int productIndex)
    {
        if (error == PurchaseError.Success)
        {
            successEvent?.Invoke(product);
        }
        else
        {
            MessageManager.Instance.SetMsg(error.ToString());
        }
    }
}