using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour
{
    [SerializeField] private Slider slProgress;
    
    private List<UniTask> _tasks = new();

    void Awake()
    {
        slProgress.value = 0;
        slProgress.DOValue(.4f, .7f)
            .SetUpdate(true);

        ResourceController.Instance.AddQueue(async delegate
        {
            await UniTask.WhenAll(_tasks);
            
            var loadScene = SceneManager.LoadSceneAsync("GamePlay", LoadSceneMode.Single);
            loadScene.allowSceneActivation = false;

            slProgress.DOKill();
            slProgress.DOValue(.8f, .7f)
                .SetUpdate(true);
            
            await UniTask.WaitUntil(() => loadScene.progress >= .7f);

            slProgress.DOKill();
            slProgress.DOValue(1, .2f)
                .OnComplete(async delegate
                {
                    loadScene.allowSceneActivation = true;
                })
                .SetUpdate(true);
        });
    }
}