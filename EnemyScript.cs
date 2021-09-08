using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemySO enemySO;
    public float health;
    public float speed;
    public bool move;
    Slider HPSlider;
    GameObject HPSliderGO;
    Camera cam;

    private void Awake()
    {
        move = true;
        health = enemySO.health;
        speed = enemySO.speed * 0.01f;
        HPSliderGO = Instantiate(enemySO.HPSliderPrefab, ObjectPool.Instance.UIContainer);
        HPSlider = HPSliderGO.GetComponent<Slider>();
        HPSlider.maxValue = health;
        cam = Camera.main;
    }
    private void OnEnable()
    {
        move = true;
        health = HPSlider.maxValue;
        HPSlider.value = health;
        HPSliderGO.transform.position = cam.WorldToScreenPoint(new Vector2(transform.position.x, transform.position.y + .2f));
        HPSliderGO.SetActive(true);
    }
    private void OnDisable()
    {
        if (HPSliderGO != null)
            HPSliderGO.SetActive(false);
    }
    private void Update()
    {
        HPSliderGO.transform.position = cam.WorldToScreenPoint(new Vector2(transform.position.x, transform.position.y + .2f));
    }
    private void FixedUpdate()
    {
        if (move)
            transform.position += (Player.Instance.transform.position - transform.position).normalized * speed;
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !Player.Instance.isInvincible)
        {
            Player.Instance.Hit(enemySO.DMG);
            Death();
        }
        if (col.CompareTag("Enemy"))
            col.GetComponent<Enemy>().TakeDamage(Time.deltaTime * 0.3f);
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        HPSlider.value = health;
        if (health <= 0)
            Death();
    }
    void Death()
    {
        GameController.Instance.EnemyDeath();
        Instantiate(enemySO.onDeathParticles, ObjectPool.Instance.test).transform.position = transform.position;
        HPSliderGO.SetActive(false);
        Instantiate(enemySO.coinsDroppedPrefab).transform.position = transform.position;
        gameObject.SetActive(false);
    }
}
