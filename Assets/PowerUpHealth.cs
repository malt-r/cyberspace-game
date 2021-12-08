using System;
using UnityEngine;

public class PowerUpHealth : BaseItem
{
    [SerializeField] 
    private String targetTag ="Player";

    [SerializeField]
    private float healAmount = 10;


    public override void Visit(Transform player)
    {
        var stats = player.gameObject.GetComponent<ActorStats>();
        stats.Heal(healAmount);
        SoundManager.PlaySound(Sound.ItemPickup);
        OnPickup();
        Destroy(gameObject);
    }
}