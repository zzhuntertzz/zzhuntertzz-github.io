using UnityEngine;

#if USING_IAP

using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Services.Core;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class MyIAPManager : MonoBehaviour, IDetailedStoreListener
{
    internal bool isInitDone = false;

    [SerializeField] private List<ProductCatalogItem> lstProduct = new();

    [Button]
    async UniTask LoadData()
    {
        var productCatalogJson = "IAPProductCatalog";
        var catalogJson = await Resources.LoadAsync<TextAsset>(productCatalogJson);
        if (!catalogJson)
        {
            Debug.LogError($">>> no json file found: iap catalog {productCatalogJson}");
            return;
        }

        if (catalogJson is not TextAsset asset)
        {
            Debug.LogError($">> catalog json type {catalogJson.GetType()}");
            return;
        }

        lstProduct.Clear();
        lstProduct = ProductCatalog.FromTextAsset(asset).allValidProducts.ToList();

        if (lstProduct.Count == 0)
        {
            Debug.LogError($">>>>>> catalog empty" +
                           $" {ProductCatalog.FromTextAsset(asset).allProducts.Count}" +
                           $" {ProductCatalog.FromTextAsset(asset).allValidProducts.Count}" +
                           $" {asset.text}");
        }
    }
    
    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
    private static Product test_product = null;
    private static Dictionary<string, ProductCatalogItem> dicProductCatalog = new();

    private Boolean return_complete = true;

    private Action<Product> onSuccess, onFailed;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
    void Awake()
    {
        // If we haven't set up the Unity Purchasing reference
        if (!isInitDone)
        // if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
        MyDebug("Complete = " + return_complete.ToString());
    }

    public static ProductCatalogItem GetCatalogItem(string id)
    {
        if (!dicProductCatalog.ContainsKey(id)) return null;
        return dicProductCatalog[id];
    }

    public Product GetProduct(string id)
    {
        return m_StoreController?.products?.WithID(id);
    }
    
    public async void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();
            MyDebug($">> Unity Gaming Services has been successfully initialized.");
            
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            dicProductCatalog.Clear();
            
            if (lstProduct.Count == 0) await LoadData();
            foreach (var product in lstProduct)
            {
                builder.AddProduct(product.id, product.type);
                if (!dicProductCatalog.ContainsKey(product.id))
                    dicProductCatalog.Add(product.id, product);
            }

            UnityPurchasing.Initialize(this, builder);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            isInitDone = true;
            throw;
        }
    }
    
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void CompletePurchase()
    {
        if (test_product == null)
            MyDebug("Cannot complete purchase, product not initialized.");
        else
        {
            m_StoreController.ConfirmPendingPurchase(test_product);
            MyDebug("Completed purchase with " + test_product.transactionID.ToString());
        }
    }

    public void ToggleComplete()
    {
        return_complete = !return_complete;
        MyDebug("Complete = " + return_complete.ToString());
    }
    public void RestorePurchases()
    {
#if UNITY_ANDROID
        m_StoreExtensionProvider.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(
            delegate(bool result, string s)
            {
                if (result)
                {
                    MyDebug("Restore purchases succeeded.");
                }
                else
                {
                    Debug.LogError("Restore purchases failed.");
                }
            });
#elif UNITY_IOS
        m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(
            delegate(bool result, string s)
            {
                if (result)
                {
                    MyDebug("Restore purchases succeeded.");
                }
                else
                {
                    MyDebug("Restore purchases failed.");
                }
            });
#endif
    }

    public void BuyProductID(string productId,
        Action<Product> onSuccess = null, Action<Product> onFailed = null)
    {
        if (!isInitDone) return;
        
        this.onSuccess = onSuccess;
        this.onFailed = onFailed;
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                MyDebug(string.Format("Purchasing product:" + product.definition.id.ToString()));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                onFailed?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError("BuyProductID FAIL. Not initialized.");
            onFailed?.Invoke(null);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        MyDebug("OnInitialized: PASS");

        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
        
        isInitDone = true;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.LogError($"OnInitializeFailed InitializationFailureReason: {error.ToString()}");
        
        
        isInitDone = true;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        OnInitializeFailed(error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        test_product = args.purchasedProduct;

        onSuccess?.Invoke(test_product);
        Debug.Log($"Purchase  Complete - Product: {test_product.definition.id}");

        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        onFailed?.Invoke(product);
        Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        OnPurchaseFailed(product, failureDescription.reason);
    }

    private void MyDebug(string debug)
    {
        Debug.LogWarning(debug);
    }
}

#else

public class Product
{
    
}

public class ProductCatalogItem
{
    public Price googlePrice;
}

public class Price
{
    public decimal value;
}

public class MyIAPManager : MonoBehaviour
{
    public static ProductCatalogItem GetCatalogItem(string id)
    {
        return null;
    }
}

#endif