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
    }

    public override void Visit(Transform transform)
    {
        var player = transform.gameObject.GetComponent<Player>();
        player.AddWeapon(weapon);
        SoundManager.PlaySound(Sound.ItemPickup);
        base.OnPickup();
        Destroy(gameObject);
    }
}