using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ButtonOpenPopup : MonoBehaviour, IButtonClick
{
    [ValueDropdown("GetFilteredTypeList")]
    [SerializeField] private string PopupName;
    
    public void ButtonClick()
    {
        if (string.IsNullOrEmpty(PopupName))
        {
            FunctionCommon.ShowNotiText(
                GameData_Localize.GetKey("comming_soon"), transform.position);
            return;
        }
        var pop = FunctionCommon.GetClass<Popup>(PopupName);
        var method = typeof(PopupManager)
            .GetMethod(nameof(PopupManager.LoadPopup))
            ?.MakeGenericMethod(pop);
        method?.Invoke(null, new[] {method.GetParameters()});
    }

#if UNITY_EDITOR
    public IEnumerable GetFilteredTypeList()
    {
        var q = typeof(Popup).Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => typeof(Popup).IsAssignableFrom(x));
        var lst = q.Select(x => x.FullName).ToList();
        lst.Add("");
        return lst;
    }
#endif
}
