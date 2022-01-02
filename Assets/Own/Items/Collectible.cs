using System;
using UnityEngine;

public class Collectible : BaseItem
{
    public override void Visit(Transform player)
    {
        handlePickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Player"))
        {
            handlePickup();
        }
    }

    private void handlePickup()
    {
        OnPickup();
        SoundManager.PlaySound(Sound.CollectiblePickup);
        this.gameObject.SetActive(false);
        //TODO: Malte MR : Hier kannst du eingreifen 
    }
}
