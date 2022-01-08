using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossActorStats : ActorStats
{
    public List<ShieldGenerator> shields;

    public GameObject ownShield;

    public override void TakeDamage(float damage, bool bomb = false, bool byEnemy = false, bool byLava= false)
    {
        
        var allDestroyed = shields.TrueForAll(generator => !generator.shieldActive);
        if (allDestroyed)
        {
            base.TakeDamage(damage, bomb, byEnemy, byLava);
        }
    }

    private void Update()
    {
        var allDestroyed = shields.TrueForAll(generator => !generator.shieldActive);
        if (allDestroyed)
        {
            ownShield.SetActive(false);
        }
    }
}
