using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public bool isInvincible { get; private set; }
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform firePoint;
    [SerializeField] ParticleSystem[] laserParticles;
    [SerializeField] Slider HP_Slider;
    [SerializeField] Slider ammo_Slider;
    [SerializeField] GameObject attackRangeCircle;
    [Header("Changeable Variables")]
    [SerializeField] float speed;
    [SerializeField] float attackRange;
    [SerializeField] float fireRate;
    [SerializeField] float ammoGenerationSpeed;
    float health;
    float DMG;
    float ammo;
    public float enemyFreeZone;
    bool laser;
    Quaternion rotateToClosestEnemy;
    GameObject closestEnemy;
    Rigidbody2D rb;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * enemyFreeZone * 2);
    }
    private void Awake()
    {
        Instance = this;
        SetOpacity(1);
        rb = GetComponent<Rigidbody2D>();
        lineRenderer.enabled = false;
        HP_Slider.maxValue = 3;
    }
    private void Update()
    {
        if (health < 0)
            return;
        rb.position += JoyStick.Instance.Movement() * speed * Time.deltaTime; // movement
        Laser();
        if (ammo < 100) // ammo generation
            ammo += Time.deltaTime * ammoGenerationSpeed;
        HP_Slider.value = health;
        ammo_Slider.value = ammo * 0.01f;
    }

    private void OnEnable()
    {
        DMG = SaveLoad.Load().DMG;
    }
    public void Laser()
    {
        if (!laser)
            return;
        closestEnemy = GetClosestEnemy();
        RotateToClosestEnemy();
        if (ammo <= 1 || closestEnemy == null || !laser)
        {
            lineRenderer.enabled = false;
            for (int i = 0; i < 4; i++)
            {
                laserParticles[i].Stop();
            }
            return;
        }
        closestEnemy.GetComponent<Enemy>().TakeDamage(Time.deltaTime * DMG);
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
    public void Hit(float DMG)
    {
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
        SetOpacity(0);
        attackRangeCircle.SetActive(false);
        GameController.Instance.OnPlayerDeath();
    }
    public void SetOpacity(float targetOpacity)
    {
        if (targetOpacity > 1 || targetOpacity < 0)
            Debug.LogError("Target opacity is not valid!");
        Material mat = GetComponent<Renderer>().material;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, targetOpacity);
    }
    public IEnumerator Invincible(float duration)
    {
        isInvincible = true;
        SetOpacity(0.1f);
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        SetOpacity(1);
    }
    public void AddAmmo(int amount)
    {
        ammo += amount;
        if (ammo > 100)
            ammo = 100;
    }
    public void GameStart()
    {
        health = 1;
        ammo = 100;
        attackRangeCircle.SetActive(true);
    }
    public void Test()
    {
        ammo += 100;
    }
    public void EnableLaser()
    {
        laser = true;
    }
    public void DisableLaser()
    {
        laser = false;
        lineRenderer.enabled = false;
        for (int i = 0; i < 4; i++)
        {
            laserParticles[i].Stop();
        }
    }
    public void Revive()
    {
        health = 1;
        SetOpacity(1);
        StartCoroutine(Invincible(2f));
        attackRangeCircle.SetActive(true);
    }
}
