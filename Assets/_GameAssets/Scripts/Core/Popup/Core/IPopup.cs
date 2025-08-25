public interface IPopup
{
    void Init();
    void Show(params object[] objects);
    void OnShow();
    void Hide();
    void OnHide();
}