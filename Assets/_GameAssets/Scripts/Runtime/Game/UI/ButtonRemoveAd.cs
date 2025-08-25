using Cysharp.Threading.Tasks;
using UnityEngine;

public class ButtonRemoveAd : MonoBehaviour
{
    private ButtonUI buttonUI;
    protected bool isShow => !PlayerData.PlayerShoppingData.HasProduct(GameFunction.PRODUCT_NO_AD);
    
    protected virtual async void Awake()
    {
        buttonUI = GetComponent<ButtonUI>();
        PlayerShoppingData.onRemoveAd += Hide;
        await UniTask.WaitUntil(() => ProductManager.isInitDone);
        buttonUI?.SetListener(RemoveAd);
        Hide();
    }

    private void OnDestroy()
    {
        PlayerShoppingData.onRemoveAd -= Hide;
    }

    protected virtual void Hide()
    {
        if (!isShow)
            PlayerShoppingData.onRemoveAd -= Hide;
        gameObject.SetActive(isShow);
    }

    public void RemoveAd()
    {
        ProductManager.Instance.BuyProductID(GameFunction.PRODUCT_NO_AD, delegate
        {
            Hide();
        }, target: transform);
    }
}