using System;
using UnityEngine;

public class PowerUpHealth : BaseItem
{
    [SerializeField] 
    private String targetTag ="Player";

    [SerializeField]
    private float healAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(targetTag))
        {
            var stats = other.gameObject.GetComponent<ActorStats>();
            stats.Heal(healAmount);
            SoundManager.PlaySound(Sound.ItemPickup);
            Destroy(gameObject);
        }
    }
}