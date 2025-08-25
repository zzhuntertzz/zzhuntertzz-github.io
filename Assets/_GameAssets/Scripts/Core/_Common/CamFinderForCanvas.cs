using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class CamFinderForCanvas : MonoBehaviour
{
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();

        SceneManager.activeSceneChanged += Search;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= Search;
    }

    private void Search(Scene arg0, Scene arg1)
    {
        if (_canvas && !_canvas.worldCamera)
            _canvas.worldCamera = FunctionCommon.mainCam;
    }
}
