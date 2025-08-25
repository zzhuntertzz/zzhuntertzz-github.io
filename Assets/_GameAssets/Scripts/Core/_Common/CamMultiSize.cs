using UnityEngine;

public class CamMultiSize : MonoBehaviour
{
    static float _ratio = 0;
    public static float newRatio
    {
        get
        {
            if (_ratio == 0)
            {
                float oldRatio = 1080f / 1920f;
                _ratio = (float) Screen.width / Screen.height;
                if (_ratio < 1)
                {
                    if (_ratio <= oldRatio)
                        _ratio /= oldRatio; //depend on width
                    else
                        _ratio = _ratio / oldRatio / 1.2f;
                }
                else
                    _ratio = 1;
            }
            return _ratio;
        }
    }
    
    private void Awake()
    {
        GetComponent<Camera>().orthographicSize /= newRatio;
        // Debug.Log(newRatio);
    }
}