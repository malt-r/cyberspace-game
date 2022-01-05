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
        scanner = GameObject.Find("/PlayerCapsule(Clone)").transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        if (!other.tag.Equals("Player")) { return; }
        scanner.SetActive(true);
        base.OnPickup();
        SoundManager.PlaySound(Sound.ItemPickup);
        Destroy(gameObject);
    }
}
