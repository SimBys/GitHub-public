using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    float speed = 0.001f;

    private void FixedUpdate()
    {
        transform.position += (Player.Instance.transform.position - transform.position).normalized * speed;
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !Player.Instance.isInvincible)
        {
                Player.Instance.Hit(1);
                GameController.score += 1;
                Player.Instance.AddAmmo(10);
                gameObject.SetActive(false);
        }
    }
}
