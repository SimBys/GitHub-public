using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;
    public static float ammo;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform firePoint;
    [SerializeField] ParticleSystem[] laserParticles;
    [SerializeField] Slider HP_Slider;
    [SerializeField] Slider ammo_Slider;
    [SerializeField] Material playerMaterial;

    [SerializeField] float speed;
    [SerializeField] float attackRange;
    [SerializeField] float fireRate;
    [SerializeField] int maxHealth;
    [SerializeField] float ammoGenerationSpeed;

    public bool isInvincible { get; };
    public bool laser { get; };
    float health;
    Quaternion rotateToClosestEnemy;
    GameObject closestEnemy;

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, attackRange);
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        lineRenderer.enabled = false;
        playerMaterial.color = new Color(playerMaterial.color.r, playerMaterial.color.g, playerMaterial.color.b, 1); // set alpha to 1
        HP_Slider.maxValue = maxHealth;
    }
    private void Update()
    {
        transform.position += (Vector3)JoyStick.Instance.Movement() * speed * Time.deltaTime; // movement
        Laser();
        if (ammo < 100) // ammo generation
            ammo += Time.deltaTime * ammoGenerationSpeed;
        HP_Slider.value = health;
        ammo_Slider.value = ammo * 0.01f;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Coin"))
            Coin(col.gameObject);
    }
    void Coin(GameObject coin)
    {
        coin.transform.parent.gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().Play();
        coin.SetActive(false);
        GameController.score += 20;
    }
    public void Laser()
    {
        closestEnemy = GetClosestEnemy();
        RotateToClosestEnemy();
        if (ammo <= 1 || closestEnemy == null)
        {
            lineRenderer.enabled = false;
            for (int i = 0; i < 4; i++)
            {
                laserParticles[i].Stop();
            }
            return;
        }
        ammo -= fireRate * Time.deltaTime;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, (Vector2)closestEnemy.transform.position);
        laserParticles[0].transform.position = (Vector2)firePoint.position;
        laserParticles[1].transform.position = (Vector2)firePoint.position;
        laserParticles[2].transform.position = (Vector2)lineRenderer.GetPosition(1);
        laserParticles[3].transform.position = (Vector2)lineRenderer.GetPosition(1);
        for (int i = 0; i < 4; i++)
        {
            if (!laserParticles[i].isPlaying)
                laserParticles[i].Play();
        }
        if (ammo < 0)
            ammo = 0;
    }
    GameObject GetClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (hits.Length == 0)
            return null;
        float currentDistance;
        float closestDistance = (transform.position - hits[0].transform.position).sqrMagnitude;
        int closestEnemyIndex = 0;
        for (int i = 1; i < hits.Length; i++)
        {
            currentDistance = (transform.position - hits[i].transform.position).sqrMagnitude;
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestEnemyIndex = i;
            }
        }
        return hits[closestEnemyIndex].gameObject;
    }
    void RotateToClosestEnemy()
    {
        if (closestEnemy == null)
            return;
        Vector2 dir = closestEnemy.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotateToClosestEnemy.eulerAngles = new Vector3(0, 0, angle);
        transform.rotation = rotateToClosestEnemy;
    }
    public void Hit(int DMG)
    {
        if (isInvincible)
            return;
        health -= DMG;
        if (health < 1)
        {
            Die();
            return;
        }
        StartCoroutine(Invincible(1));
    }
    void Die()
    {
        GameController.Instance.OnPlayerDeath();
        gameObject.SetActive(false);
    }
    IEnumerator Invincible(float duration)
    {
        isInvincible = true;
        playerMaterial.color = new Color(playerMaterial.color.r, playerMaterial.color.g, playerMaterial.color.b, 0.1f);
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        playerMaterial.color = new Color(playerMaterial.color.r, playerMaterial.color.g, playerMaterial.color.b, 1);
    }
    public void AddAmmo(int amount) // add ammo when enemy is killed
    {
        ammo += amount;
        if (ammo > 100)
            ammo = 100;
    }
    public void GameStart()
    {
        ammo = 100;
        health = 3;
    }
    public void Test()
    {
        ammo += 100;
    }
}
