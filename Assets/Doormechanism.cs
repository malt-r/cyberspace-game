using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doormechanism : MonoBehaviour
{
    public doorNavMarker Door1;
    public doorNavMarker Door2;

    public GameObject[] enemies;

    private bool done = false;

    private 
        
    // Start is called before the first frame update
    void Start()
    {
        openDoors();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || done)
        {
            return;
        }

        other.GetComponent<ActorStats>().OnHealthReachedZero += openDoors;
        closeDoors();
        
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || done)
        {
            return;
        }
        openDoors();
    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) { return;}
        
        if (areAllEnemiesDead())
        {
            openDoors();
        }
    }

    private void openDoors()
    {
        Door1.setDoorActive(false);
        Door2.setDoorActive(false);
    }

    private void closeDoors()
    {
        Door1.setDoorActive(true);
        Door2.setDoorActive(true);
    }

    private bool areAllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeSelf)
            {
                return false;
            } 
        }
        return true;
    }
}
