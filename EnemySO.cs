using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemySO : ScriptableObject
{
    public int health;
    public int speed;
    public int DMG;
    public GameObject HPSliderPrefab;
    public GameObject onDeathParticles;
    public GameObject coinsDroppedPrefab;
    public int coinsDropped;

}
