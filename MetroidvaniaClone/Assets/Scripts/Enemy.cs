using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected LayerMask wallObjectLayer;

    [Header("Recoil Settings")]
    [SerializeField] protected float recoilLenght;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoilling = false;



    // References
    protected Rigidbody2D rb;
    protected float recoilTimer;
    protected PlayerController player;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }

        if (isRecoilling)
        {
            if (recoilTimer < recoilLenght)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoilling = false;
                recoilTimer = 0;
            }
        }
    }

    protected bool IsWalkable(Vector3 _position)
    {
        if (Physics2D.OverlapCircle(_position, 0.2f, wallObjectLayer) != null)
        {
            return false;
        }
        return true;
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoilling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            isRecoilling = true;
        }
    }

    protected void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !PlayerController.Instance.playerState.invincible)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamege(damage);
    }
}
