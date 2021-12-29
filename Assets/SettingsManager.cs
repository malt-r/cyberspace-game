using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
  private static SettingsManager _instance;
  public static SettingsManager Instance => _instance;

  public float MouseSensitivity = 5f;

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

  public void SetMouseSensitivity(float sensitivity)
  {
    PlayerPrefs.SetFloat("Mouse/Sensitivity",sensitivity);
    EventManager.TriggerEvent("Mouse/SensitivityChanged",sensitivity);
  }


  
}
