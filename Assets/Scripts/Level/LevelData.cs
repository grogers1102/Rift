using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public GameObject gunPrefab;
    public GameObject meleeWeaponPrefab;
    public int maxHeals = 5;
    public float playerMaxHealth = 100f;
    // Add any other level-specific configurations here
} 