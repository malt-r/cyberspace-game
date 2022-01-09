using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShieldGenerator : MonoBehaviour
{
    [SerializeField] private AudioClip startupSound;
    
    public bool shieldActive = true;

    public bool wasActivated = false;
    
    public GameObject activeShield;
    public GameObject inactiveShield;

    public event Action OnShieldDestroy;

    // Update is called once per frame
    private void Start()
    {
        activeShield.GetComponent<ActorStats>().OnHealthReachedZero += () =>
        {
            shieldActive = false;
            OnShieldDestroy?.Invoke();
            inactiveShield.gameObject.SetActive(true);
        };
    }

    public void PlayStartupAnimation()
    {
        GetComponent<Animator>().SetTrigger("Startup");
        GetComponent<AudioSource>().PlayOneShot(startupSound);
        wasActivated = true;
    }
}
