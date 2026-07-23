using UnityEngine;

public class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

    public static void InstantiatePrefabs()
    {
        Debug.Log(".......Instantiating global objects......");

            GameObject[] prefabToInstatiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach(GameObject prefab in prefabToInstatiate)
        {
            Debug.Log($"Creating {prefab.name}");

            GameObject.Instantiate(prefab);
        }

        Debug.Log(".......Instantiating global objects  done");
    }
}
