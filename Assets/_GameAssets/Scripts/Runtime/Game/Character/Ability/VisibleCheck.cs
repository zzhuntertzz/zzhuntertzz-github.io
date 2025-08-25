using UnityEngine;

public class VisibleCheck : MonoBehaviour, IAssign
{
    private IVisibleCheck visibleCheck;
    
    private void OnBecameVisible()
    {
        visibleCheck?.OnVisible();
    }

    private void OnBecameInvisible()
    {
        visibleCheck?.OnInVisible();
    }

    public void Assign(object value)
    {
        if (value is not IVisibleCheck visibleCheck) return;
        this.visibleCheck = visibleCheck;
    }
}