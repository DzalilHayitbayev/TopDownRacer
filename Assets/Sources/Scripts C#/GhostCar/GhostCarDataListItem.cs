using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostCarDataListItem : ISerializationCallbackReceiver
{
    [System.NonSerialized]
    public Vector2 position = Vector2.zero;

    [System.NonSerialized]
    public float rotationZ = 0f;

    [System.NonSerialized]
    public float timeSinceLevelLoaded = 0;

    [System.NonSerialized]
    public Vector3 localScale = Vector3.one;

    [SerializeField]
    int x = 0;

    [SerializeField]
    int y = 0;

    [SerializeField]
    int r = 0;

    [SerializeField]
    int t = 0;

    [SerializeField]
    int s = 0;

    public GhostCarDataListItem(Vector2 position_, float rotation_, Vector3 localScale_, float timeSinceLevelLoaded_) 
    {
        position = position_;
        rotationZ = rotation_;
        timeSinceLevelLoaded = timeSinceLevelLoaded_;
        localScale = localScale_;
    }

    public void OnBeforeSerialize()
    {
        t =(int)(timeSinceLevelLoaded * 1000.0f);

        x = (int)(position.x * 1000.0f);
        y = (int)(position.y * 1000.0f);

        s = (int)(localScale.x * 1000.0f);

        r = Mathf.RoundToInt(rotationZ);
    }

    public void OnAfterDeserialize() 
    {
        timeSinceLevelLoaded = t / 1000.0f;
        position.x = x / 1000.0f;
        position.y = y / 1000.0f;
        localScale = new Vector3(s / 1000.0f, s / 1000.0f, s / 1000.0f);

        rotationZ = r;
    }
}
