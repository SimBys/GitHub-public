using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[DefaultExecutionOrder(-100)]
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    [SerializeField] EnemySO[] enemyScriptableObjects;

    List<GameObject> enemys = new List<GameObject>();

    private void Awake() // singleton
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    public GameObject GetEnemy(int enemyIndex)
    {
        for (int i = 0; i < enemys.Count; i++)
        {
            if (!enemys[i].activeInHierarchy)
                return enemys[i];
        }
        GameObject GO = Instantiate(enemyScriptableObjects[enemyIndex].prefab, transform);
        GO.AddComponent<EnemyScript>();
        enemys.Add(GO);
        return GO;
    }
}