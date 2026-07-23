using System;
using UnityEngine;

[System.Serializable]
public struct CarColorScheme
{
    [SerializeField] private Sprite colorSprite;
    [SerializeField] private int price;

    public Sprite ColorSprite => colorSprite;
    public int Price => price;
}

[CreateAssetMenu(fileName = "New Car Data", menuName = "Car Data", order = 51)]
public class CarData : ScriptableObject
{
    [SerializeField] private int carUniqueID = 0;
    [SerializeField] private int price = 0; // Цена самой машины
    [SerializeField] private CarColorScheme[] carColorSchemes;
    [SerializeField] private GameObject carPrefab;

    public int CarUniqueID => carUniqueID;
    public int Price => price;
    public CarColorScheme[] CarColorSchemes => carColorSchemes;
    public GameObject CarPrefab => carPrefab;

    public Sprite[] GetColorSprites()
    {
        if (carColorSchemes == null) return new Sprite[0];

        Sprite[] sprites = new Sprite[carColorSchemes.Length];
        for (int i = 0; i < carColorSchemes.Length; i++)
        {
            sprites[i] = carColorSchemes[i].ColorSprite;
        }
        return sprites;
    }
}