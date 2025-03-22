using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Enemy
{

    protected override void Awake()
    {
        base.Awake();
    }
 
    protected override void Start()
    {
        rb.gravityScale = 12f;
    }

    protected override void Update()
    {
        base.Update();

        // isn't in push back
        if (!isRecoilling)
        {
            // follow the player
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(PlayerController.Instance.transform.position.x, transform.position.y), speed * Time.deltaTime);
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}
