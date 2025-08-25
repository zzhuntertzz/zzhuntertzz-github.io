using Lean.Pool;

namespace UnityEngine.UI
{
    [System.Serializable]
    public class LoopScrollPrefabSource
    {
        public string prefabName;

        private GameObject pref;

        public async void Init()
        {
            ResourceController.Instance.AddQueue(async delegate
            {
                pref = await A.Get<GameObject>(prefabName);
            });
        }

        public bool IsLoadDone()
        {
            return pref is not null;
        }
        
        public virtual GameObject GetObject()
        {
            return LeanPool.Spawn(pref);
        }

        public virtual void ReturnObject(Transform go)
        {
            go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            LeanPool.Despawn(go);
        }
    }
}
