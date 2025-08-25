using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageUI : MonoBehaviour, ISetSprite
{
    private Image _image;

    private Image Image
    {
        get
        {
            if (!_image) _image = GetComponent<Image>();
            return _image;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
        Image.SetNativeSize();
    }
}
