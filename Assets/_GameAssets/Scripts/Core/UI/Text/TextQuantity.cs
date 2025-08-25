using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextQuantity : MonoBehaviour
{
    [SerializeField] private string itemId;
    
    private int value;
    private TMP_Text txtQuantity;
    private List<Tween> tws = new();
    private Color oldClr;
    private Vector3 oldScale;

    private void Start()
    {
        txtQuantity = GetComponent<TMP_Text>();
        oldClr = txtQuantity.color;
        oldScale = txtQuantity.transform.localScale;
        PlayerInventory.OnQuantityChanged += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        PlayerInventory.OnQuantityChanged -= UpdateText;
    }

    void UpdateText()
    {
        value = PlayerData.PlayerInventory.GetQuantity(itemId);
        txtQuantity.text = value.AbbreviateNumber();
    }

    void UpdateText(string id, int quantity, int value)
    {
        if (id != itemId) return;
        if (quantity == value) return;
        ClearOldTws();
        
        var clr= quantity > value ?
            FunctionCommon.COLOR_HEX_GREEN.ToColor() :
            FunctionCommon.COLOR_HEX_RED.ToColor();
        txtQuantity.color = clr;
        var spd = .3f;

        void ResetClr()
        {
            txtQuantity.color = oldClr;
        }
        tws.Add(
            FunctionCommon.ChangeValueInt(value, quantity,
                    spd, delegate(int val)
                    {
                        txtQuantity.text = val.AbbreviateNumber();
                    })
                .OnComplete(ResetClr)
                .OnKill(ResetClr)
                .SetUpdate(true)
        );

        void ResetScale()
        {
            transform.localScale = oldScale;
        }
        tws.Add(
            transform.DOScale(oldScale + Vector3.one * 0.1f, spd / 2f)
                .OnComplete(delegate
                {
                    transform.DOScale(oldScale, spd / 2f)
                        .SetUpdate(true)
                        .OnKill(ResetScale);
                })
                .SetUpdate(true)
                .OnKill(ResetScale)
            );
        value = quantity;
    }

    void ClearOldTws()
    {
        foreach (var tw in tws)
        {
            tw?.Kill();
        }
        tws.Clear();
    }
}