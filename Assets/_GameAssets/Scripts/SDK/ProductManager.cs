using System;
using UnityEngine;

#if USING_IAP

using UnityEngine.Purchasing;

namespace Tutti.G
{
    [RequireComponent(typeof(MyIAPManager))]
    public class ProductManager : SinglePrivaton<ProductManager>
    {   
        public static event Action<Product> onPurchaseProduct = delegate { };
        public static bool isInitDone => Instance._iapManager.isInitDone;
        private MyIAPManager _iapManager;

        async void Awake()
        {
            _iapManager = gameObject.GetOrAddComponent<MyIAPManager>();
            await UniTask.WaitUntil(() => isInitDone);
            HasProduct(GameFunction.PRODUCT_NO_AD);
        }

        public Product GetProduct(string id)
        {
            return _iapManager.GetProduct(id);
        }

        public bool HasProduct(string id)
        {
            if (id.Contains(GameFunction.PRODUCT_NO_AD))
                id = GameFunction.PRODUCT_NO_AD;
            if (PlayerData.PlayerShoppingData.HasProduct(id))
                return true;
            if (_iapManager.GetProduct(id) is { } product)
            {
                if (product.definition.type == ProductType.Consumable)
                {
                    return false;
                }
                if (!product.hasReceipt)
                    return false;
                else
                {
                    PlayerData.PlayerShoppingData.AddProduct(id);
                    return true;
                }
            }
            return true;
        }

        public void BuyProductID(string id,
            Action<Product> onSuccess = null, Action<Product> onFailed = null,
            Transform target = null)
        {
            void OnPurchaseSuccess(Product product)
            {
                if (product.definition.type != ProductType.Consumable)
                {
                    if (id.Contains(GameFunction.PRODUCT_NO_AD))
                        PlayerData.PlayerShoppingData.AddProduct(GameFunction.PRODUCT_NO_AD);
                    else
                        PlayerData.PlayerShoppingData.AddProduct(id);
                }
                else
                {
                    PlayerData.PlayerShoppingData.PurchaseProduct(id);
                }

                onSuccess?.Invoke(product);
                onPurchaseProduct(product);
                if (!Debug.isDebugBuild)
                {
                    // new ABIEventAFPurchase()
                    // {
                    //     product = product,
                    // }.Post();
                    new EventProductPurchaseSuccess()
                    {
                        product = product,
                    }.Post();
                }
            }

            if (Debug.isDebugBuild)
            {
                var product = GetProduct(id);
                OnPurchaseSuccess(product);
                return;
            }

            if (onFailed is null && target is not null)
            {
                onFailed = delegate (Product product)
                {
                    GameFunction.ShowNotiText($"Failed to purchase",
                        target.position);
                    new EventProductPurchaseFail()
                    {
                        product = product,
                    }.Post();
                };
            }
            _iapManager.BuyProductID(id, OnPurchaseSuccess, onFailed);
            new EventProductPurchaseClick()
            {
                product_id = id,
            }.Post();
        }
    }   
}

#else

[RequireComponent(typeof(MyIAPManager))]
public class ProductManager : SinglePrivaton<ProductManager>
{
    public static bool isInitDone = true;

    public void BuyProductID(string id,
        Action<Product> onSuccess = null, Action<Product> onFailed = null,
        Transform target = null)
    {
    }
}

#endif