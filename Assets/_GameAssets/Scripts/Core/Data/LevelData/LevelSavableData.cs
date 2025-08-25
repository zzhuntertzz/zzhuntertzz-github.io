using System;
using UnityEngine;

[Serializable]
public class LevelSavableData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public string name;
    public string parentName;

    public virtual void FromGameObject(GameObject go)
    {
        Transform t = go.transform;
        position = t.position;
        rotation = t.rotation;
        scale = t.localScale;
        name = go.name.GetName();
        parentName = t.parent.name.GetName();
    }
}

[Serializable]
public class LevelSavableSpriteRenderer : LevelSavableData
{
    public string spriteName;
    public Color color;

    public override void FromGameObject(GameObject go)
    {
        base.FromGameObject(go);
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteName = sr.sprite != null ? sr.sprite.name : null;
            color = sr.color;
        }
    }
}