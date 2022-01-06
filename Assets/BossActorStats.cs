using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossActorStats : ActorStats
{
    public List<ShieldGenerator> shields;


    public override void TakeDamage(float damage, bool bomb = false)
    {
        var allDestroyed = shields.TrueForAll(generator => !generator.shieldActive);

        if (allDestroyed)
        {
            base.TakeDamage(damage, bomb);
        }
    }

    private void Update()
    {
        
    }
}
