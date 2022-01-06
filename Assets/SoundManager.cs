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
    SetBackgroundMusic("Game");
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
    AudioMixer.SetFloat("MainVolume", linearToDb(newVolume));
  }
  public void SetMusicVolume(float newVolume)
  {
    AudioMixer.SetFloat("MusicVolume",linearToDb(newVolume));
  }
  
  public void SetVoiceVolume(float newVolume)
  {
    AudioMixer.SetFloat("VoiceVolume", linearToDb(newVolume));
  }
  
  public void SetSoundEffectVolume(float newVolume)
  {
    AudioMixer.SetFloat("SoundeffectVolume", linearToDb(newVolume));
  }

  public static float linearToDb(float linearVolume)
  {
    var dbVolume = Mathf.Log10(linearVolume) * 20;
    if (linearVolume == 0.0f)
    {
      dbVolume = -80.0f;
    }

    return dbVolume;
  }
  
  public static float dbToLinear(float linearVolume)
  {
    return Mathf.Pow(10.0f, linearVolume / 20.0f);;
  }
  public static void PlaySound(Sound sound)
  {
    if (sound == null) return;
    var soundGameObject = new GameObject(sound.ToString());
    var audioSource = soundGameObject.AddComponent<AudioSource>();
    var soundAudioClip = GetSoundAudioClip(sound);
    if (soundAudioClip == null) return;

    Instance.AudioMixer.GetFloat("SoundeffectVolume", out var volume);
    var finalVolume = dbToLinear(volume);
    audioSource.PlayOneShot(soundAudioClip.clip,finalVolume);
    Destroy(soundGameObject,30);
  }

  private static SoundAudioClip GetSoundAudioClip(Sound sound)
  {
    if (Instance == null) return null;
    foreach (var soundAudioclip in Instance.audioClips)
    {
      if (soundAudioclip.sound == sound)
      {
        return soundAudioclip;
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

//Obacht: Das ändern der Enumreihenfolge macht die Zuordnung in der Szene kaputt!
public enum Sound
{
  PlayerHit,
  EnemyHit,
  EnemyDie,
  ItemPickup,
  CollectiblePickup,
  GUICLick,
  }


[Serializable]
public class SoundAudioClip {
  public Sound sound;
  public AudioClip clip;
  public float volume;
}