using UnityEngine;

[CreateAssetMenu(fileName = "New Car Data", menuName = "Car Data", order = 51)]
public class CarData : ScriptableObject
{
    [SerializeField]
    private int carUniqueID = 0;

    [SerializeField]
    private Sprite[] carColorSchemes;

    [SerializeField]
    private GameObject carPrefab;

    public int CarUniqueID
    {
        get { return carUniqueID; }
    }
    public Sprite[] CarColorSchemes
    {
        get { return carColorSchemes; }
    }

    public GameObject CarPrefab
    {
        get { return carPrefab; }
    }

}