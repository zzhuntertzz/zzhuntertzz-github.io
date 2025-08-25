using TMPro;
using UnityEngine;

public class TextLocalize : MonoBehaviour
{
    [SerializeField] string key;
    private TMP_Text txt;
    
    private void Start()
    {
        if (string.IsNullOrEmpty(key)) return;
        txt = GetComponent<TMP_Text>();
        if (!txt) return;
        ResourceController.Instance.AddQueue(delegate
        {
            OnChangeLanguage(default);
        });
    }

    private void OnEnable()
    {
        GameData_Localize.OnChangeLanguage += OnChangeLanguage;
    }

    private void OnDisable()
    {
        GameData_Localize.OnChangeLanguage -= OnChangeLanguage;
    }

    private void OnChangeLanguage(string language)
    {
        if (txt)
            txt.text = GameData_Localize.GetKey(key);
    }
}