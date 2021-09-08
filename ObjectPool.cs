using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[DefaultExecutionOrder(-100)]
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] Material enemyMat;
    [SerializeField] Transform container;
    public Transform UIContainer; // UI pooling
    public Transform test;

    List<GameObject> enemies = new List<GameObject>();

    private void Awake() // singleton
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    public GameObject GetEnemy(int enemyIndex)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].activeInHierarchy)
                return enemies[i];
        }
        GameObject GO = Instantiate(enemyPrefabs[enemyIndex], container);
        enemies.Add(GO);
        GO.GetComponent<Renderer>().material = enemyMat;
        return GO;
    }
    public Enemy[] GetAllEnemies()
    {
        Enemy[] tempArray = new Enemy[container.childCount];
        for (int i = 0; i < container.childCount; i++)
        {
            tempArray[i] = container.GetChild(i).GetComponent<Enemy>();
        }
        return tempArray;
    }
}