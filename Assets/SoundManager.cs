using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundManager : MonoBehaviour
{
  private static SoundManager _instance;
  public static SoundManager Instance => _instance;

  public AudioMixer AudioMixer;

  private void Awake()
  {
    if (_instance != null && _instance != this) 
    { 
      Destroy(this.gameObject);
      return;
    }

    _instance = this;
    DontDestroyOnLoad(this.gameObject);
    
  }

  public void SetMainVolume(float newVolume)
  {
    AudioMixer.SetFloat("MainVolume", newVolume);
  }
  public void SetMusicVolume(float newVolume)
  {
    AudioMixer.SetFloat("MusicVolume", newVolume);
  }
  
  public void SetSoundEffectVolume(float newVolume)
  {
    AudioMixer.SetFloat("SoundeffectVolume", newVolume);
  }
  
  
  public static void PlaySound(string soundName="Sound")
  {
    var soundGameObject = new GameObject(soundName);
    var audioSource = soundGameObject.GetComponent<AudioSource>();
  }
}
