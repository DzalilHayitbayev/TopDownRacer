using UnityEngine;

public enum PowerUpType
{
    Boost,
    Shield,
    Repair,
    ShockWave,
    Shoot,
    Mine
}

[CreateAssetMenu(fileName = "New PowerUp", menuName = "PowerUps/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    [Header("General Info")]
    public PowerUpType type;
    public string powerUpName;
    public string title;
    public Sprite icon;
    public int price = 100;

    [Header("Settings")]
    public float duration = 5f;       
    public int ammo = 3;              
    public float value = 50f;         
    public GameObject prefab;         
}