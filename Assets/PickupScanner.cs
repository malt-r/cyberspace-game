using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScanner : MonoBehaviour
{
    [SerializeField] private GameObject scanner;
    // Start is called before the first frame update
    void Start()
    {
        scanner = GameObject.Find("/PlayerCapsule").transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player")) { return; }
        scanner.SetActive(true);
        SoundManager.PlaySound(Sound.ItemPickup);
        Destroy(gameObject);
    }
}
