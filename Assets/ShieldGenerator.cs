using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShieldGenerator : MonoBehaviour
{
    public bool shieldActive = true;
    public GameObject activeShield;
    public GameObject inactiveShield;

    // Update is called once per frame
    private void Start()
    {
        activeShield.GetComponent<ActorStats>().OnHealthReachedZero += () =>
        {
            shieldActive = false;
            inactiveShield.gameObject.SetActive(true);
        };
        
    }
}
