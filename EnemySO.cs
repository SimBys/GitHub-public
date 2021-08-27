using UnityEngine;

//[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemySO : ScriptableObject
{
    public int health;
    public int speed;
    public int DMG;
    public GameObject prefab;
    public GameObject coinsDroppedPrefab;
    public int coinsDropped;
}
