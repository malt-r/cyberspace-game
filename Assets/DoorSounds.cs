using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSounds : MonoBehaviour
{
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioSource source;

    public void PlayOpen()
    {
        source.PlayOneShot(openSound);
    }

    public void PlayClose()
    {
        source.PlayOneShot(closeSound);
    }
}
