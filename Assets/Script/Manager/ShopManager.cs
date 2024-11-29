using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum ProductType
{
    None = 0, // 예외처리
    HINT, // 힌트
    THEME, // 테마
}

public class ProductTable : ClientTable
{
    public int productIdx;
    public ProductType productType;
    public int itemIdx;
    public int itemValue;
    public float itemCost;
}

public enum PurchaseError
{
    Success = 0, // 무사 구매 성공
    FailInvalidProduct, // 유효하지 않은 상품
    FailInAppCancel, // 인앱 구매 취소 
}

public class ShopManager : SingletonTemplate<ShopManager>
{
    private Dictionary<int, ProductTable> productMap = new Dictionary<int, ProductTable>();
    private Dictionary<ProductType, int> productTypeKeyMap = new Dictionary<ProductType, int>(); // 키 값 = 상품 타입
    private Dictionary<int, int> supplyProductMap = new Dictionary<int, int>(); // 키 값 = 구매시 제공 아이템 번호
    
    [Serializable]
    public class ProductEvent : UnityEvent<PurchaseError, int>{}
    
    [FormerlySerializedAs("OnPurchaseProductEvent")]
    [SerializeField]
    public ProductEvent mOnPurchaseProduct = new ProductEvent();
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void InitTable()
    {
        InitProductTable();
    }

    #region Product
    private void InitProductTable()
    {
        List<ProductTable> productTableList = ClientTableManager.LoadTable<ProductTable>("Product");
        productMap.Clear();
        productTypeKeyMap.Clear();
        supplyProductMap.Clear();
        if (productTableList == null)
        {
            return;
        }

        for (int i = 0; i < productTableList.Count; i++)
        {
            ProductTable table = productTableList[i];
            if (!productMap.ContainsKey(table.productIdx))
            {
                productMap.Add(table.productIdx, table);

                if (!productTypeKeyMap.ContainsKey(table.productType))
                {
                    productTypeKeyMap.Add(table.productType, table.productIdx);
                }

                if (!supplyProductMap.ContainsKey(table.itemIdx))
                {
                    supplyProductMap.Add(table.itemIdx, table.productIdx);
                }
            }
        }
    }

    public int ProductTypeToIndex(ProductType type)
    {
        if (productTypeKeyMap.ContainsKey(type))
        {
            return productTypeKeyMap[type];
        }

        return 0;
    }

    public int SupplyItemIndexToProductIndex(int itemIndex)
    {
        if (supplyProductMap.ContainsKey(itemIndex))
        {
            return supplyProductMap[itemIndex];
        }

        return 0;
    }

    public void RequestBuyProduct(int index, UnityAction<PurchaseError, int> purchaseEvent)
    {
        if (mOnPurchaseProduct == null)
        {
            mOnPurchaseProduct = new ProductEvent();
        }
        
        mOnPurchaseProduct.RemoveAllListeners();
        if (purchaseEvent != null)
        {
            mOnPurchaseProduct.AddListener(purchaseEvent);
        }

        if (!productMap.ContainsKey(index))
        {
            OnFailBuyProduct(PurchaseError.FailInvalidProduct, index);
            return;
        }
        
        // 구매 코드 작성 필요

        OnSuccessBuyProduct(index); // 일단 구매 성공을 바로 날림
    }

    public void RequestBuyProduct(ProductType type, UnityAction<PurchaseError, int> purchaseEvent)
    {
        RequestBuyProduct(ProductTypeToIndex(type), purchaseEvent);
    }

    public void RequestBuyProductBySupplyItemIndex(int itemIndex, UnityAction<PurchaseError, int> purchaseEvent)
    {
        RequestBuyProduct(SupplyItemIndexToProductIndex(itemIndex), purchaseEvent);
    }

    private void OnSuccessBuyProduct(int index)
    {
        ProductTable product = GetProductTable(index);
        if (product == null)
        {
            OnFailBuyProduct(PurchaseError.FailInvalidProduct, index);
            return;
        }
        
        // 실제 내부 아이템 증가 처리
        ItemManager.Instance.AddHaveItemCount(product.itemIdx, product.itemValue);
        ItemManager.Instance.SaveHaveItem();
        
        mOnPurchaseProduct?.Invoke(PurchaseError.Success, index);
    }

    private void OnFailBuyProduct(PurchaseError error, int index)
    {
        mOnPurchaseProduct?.Invoke(error, index);
    }

    public ProductTable GetProductTable(int index)
    {
        if (productMap.ContainsKey(index))
        {
            return productMap[index];
        }

        return null;
    }
    #endregion
}