using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;
using UnityTimer;

public class TextNoti : MonoBehaviour, ISetData
{
    [SerializeField] private TMP_Text txtNoti;
    [SerializeField] private float showTime, floatPosY;

    private void Awake()
    {
        txtNoti.text = "";
    }

    public void SetData(object objData)
    {
        if (objData is string content)
        {
            txtNoti.text = content;
        }
        
        txtNoti.rectTransform.anchoredPosition = Vector2.zero;
        txtNoti.rectTransform.DOAnchorPosY(
                txtNoti.rectTransform.anchoredPosition.y + floatPosY,
                showTime)
            .SetEase(Ease.Linear)
            .SetUpdate(true);
        txtNoti.DOFade(1, 0)
            .OnComplete(delegate
            {
                txtNoti.DOFade(0, showTime)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);
            })
            .SetUpdate(true);
        
        this.AttachTimer(showTime, delegate
        {
            LeanPool.Despawn(gameObject);
        });
    }
}