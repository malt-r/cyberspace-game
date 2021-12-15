using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundManager : MonoBehaviour
{
  private static SoundManager _instance;
  public static SoundManager Instance => _instance;

  public AudioMixer AudioMixer;


  public AudioClip MenueSound;
  public AudioClip BackgroundSound;

  private static AudioSource audioSource;

  public SoundAudioClip[] audioClips;

  private void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    SetBackgroundMusic("Menue");
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
  
  
  public static void PlaySound(Sound sound)
  {
    var soundGameObject = new GameObject(sound.ToString());
    var audioSource = soundGameObject.AddComponent<AudioSource>();
    audioSource.PlayOneShot(GetAudioClip(sound));
  }

  private static AudioClip GetAudioClip(Sound sound)
  {
    foreach (var soundAudioclip in Instance.audioClips)
    {
      if (soundAudioclip.sound == sound)
      {
        return soundAudioclip.clip;
      }
    }

    Debug.LogError("Sound "+sound +" not found!");
    return null;
  }
  public void SetBackgroundMusic(string musicName)
  {
    switch (musicName)
    {
      case "Menue":
        audioSource.clip = MenueSound;
        break;
      default :
        audioSource.clip = BackgroundSound;
        break;
    }
    audioSource.Play();
  }
}

public enum Sound
{
  PlayerMove,
  PlayerHit,
  EnemyHit,
  EnemyDie,
  ItemPickup
}


[Serializable]
public class SoundAudioClip {
  public Sound sound;
  public AudioClip clip;
}