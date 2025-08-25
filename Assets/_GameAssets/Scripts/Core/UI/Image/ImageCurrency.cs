using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageCurrency : MonoBehaviour
{
    [SerializeField] private int currencyId;
    [SerializeField] private bool isSetNative = true;
    
    private Image imgPrice;
    
    void Awake()
    {
        imgPrice = GetComponent<Image>();
        ResourceController.Instance.AddQueue(async delegate
        {
            imgPrice.sprite = await MyKeys.Atlas.GetSprite(
                MyKeys.Atlas.Currency, currencyId.ToString());
            if (isSetNative) imgPrice.SetNativeSize();
        });
    }
}