using System;
using Assets.Weapons;
using UnityEngine;

public class PowerUpWeapon : BaseItem
{
    [SerializeField] 
    private String targetTag ="Player";
    public BaseWeapon weapon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(targetTag))
        {
            var player = other.gameObject.GetComponent<Player>();
            player.AddWeapon(weapon);
            SoundManager.PlaySound(Sound.ItemPickup);
            Destroy(gameObject);
        }
    }
}