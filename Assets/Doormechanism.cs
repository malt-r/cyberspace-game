using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Doormechanism : MonoBehaviour
{
    [SerializeField] 
    public doorNavMarker[] Doors;

    [SerializeField] 
    private bool StartWithDoorsOpen = true;

    public GameObject[] enemies;

    private bool done = false;
    private bool open = true;
    private bool audioActive = true;
        
    // Start is called before the first frame update
    protected void Start()
    {
        if (!StartWithDoorsOpen) // door will start in open state (set by animator)
        {
            closeDoors(true);
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

        if (areAllEnemiesDead())
        {
            return;
        }

        other.GetComponent<ActorStats>().OnHealthReachedZero += openDoors;
        closeDoors();
    }

    public void openDoors()
    {
        openDoors(false);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || done)
        {
            return;
        }

        if (open)
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

    private void SetAudioState(bool deactivateAudio)
    {
        if (audioActive && deactivateAudio)
        {
            foreach (var door in Doors)
            {
                door.GetComponent<AudioSource>().enabled = false;
            }

            audioActive = false;
        }
        else if (!audioActive && !deactivateAudio)
        {
            foreach (var door in Doors)
            {
                door.GetComponent<AudioSource>().enabled = true;
            }

            audioActive = true;
        }
    }

    public void openDoors(bool deactivateAudio = false)
    {
        SetAudioState(deactivateAudio);
        
        foreach (var door in Doors)
        {
            door.GetComponent<Animator>().SetTrigger("Open");
        }

        open = true;
    }

    public void closeDoors(bool deactivateAudio = false)
    {
        SetAudioState(deactivateAudio);
        
        foreach (var door in Doors)
        {
            door.setDoorActive(true);
            door.GetComponent<Animator>().SetTrigger("Close");
        }

        open = false;
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
