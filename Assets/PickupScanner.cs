using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScanner : BaseItem
{
    [SerializeField] private GameObject scanner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Visit(Transform player)
    {
        //throw new NotImplementedException();
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        scanner = player.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        if (!other.tag.Equals("Player")) { return; }
        scanner.SetActive(true);
        player.GetComponent<WeaponControl>().enabled = true;
        base.OnPickup();
        SoundManager.PlaySound(Sound.ItemPickup);
        Destroy(gameObject);
    }
}
