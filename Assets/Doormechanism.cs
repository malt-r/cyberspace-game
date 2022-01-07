using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doormechanism : MonoBehaviour
{
    [SerializeField] 
    public doorNavMarker[] Doors;

    [SerializeField] 
    private bool StartWithDoorsOpen = true;

    public GameObject[] enemies;

    private bool done = false;
        
    // Start is called before the first frame update
    protected void Start()
    {
        if (!StartWithDoorsOpen) // door will start in open state (set by animator)
        {
            closeDoors();
        }
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

    public void openDoors()
    {
        foreach (var door in Doors)
        {
            door.GetComponent<Animator>().SetTrigger("Open");
        }
    }

    public void closeDoors()
    {
        foreach (var door in Doors)
        {
            door.setDoorActive(true);
            door.GetComponent<Animator>().SetTrigger("Close");
        }
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
