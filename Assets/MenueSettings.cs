using System;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenueSettings : MonoBehaviour
{
    [SerializeField]
    private Slider mainVolumeSlider;
    [SerializeField]
    private Slider musicVolumeSlider;
    [SerializeField]
    private Slider voiceVolumeSlider;
    [SerializeField]
    private Slider soundeffectsVolumeSlider;
    [SerializeField]
    private Slider mouseSensitivitySlider;

    [SerializeField] private Button backButton;


    void Awake()
    {
        Debug.Log("Awake");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        //Sound
        initSlider(mainVolumeSlider, "MainVolume");
        initSlider(musicVolumeSlider,"MusicVolume");
        initSlider(voiceVolumeSlider,"VoiceVolume");
        initSlider(soundeffectsVolumeSlider,"SoundeffectVolume");
        
        
        mainVolumeSlider.onValueChanged.AddListener((float newValue) => { handleSoundSettings(mainVolumeSlider, newValue); });
        musicVolumeSlider.onValueChanged.AddListener((float newValue) => { handleSoundSettings(musicVolumeSlider, newValue); });
        voiceVolumeSlider.onValueChanged.AddListener((float newValue) => { handleSoundSettings(voiceVolumeSlider, newValue); });
        soundeffectsVolumeSlider.onValueChanged.AddListener((float newValue) => { handleSoundSettings(soundeffectsVolumeSlider, newValue); });
        
        //Mouse,
        initMouseSensitivitySlider(mouseSensitivitySlider);
        mouseSensitivitySlider.onValueChanged.AddListener((float newValue) => { handleMouseSettings(mouseSensitivitySlider, newValue); });
       

        backButton.onClick.AddListener(() =>
        {
            var menueInteractor = FindObjectOfType(typeof(MenueInteractor)) as MenueInteractor;
            if(menueInteractor){
                menueInteractor.DisableMenue();
            }
        });

        
    }

    private void initMouseSensitivitySlider(Slider slider)
    {
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat("Mouse/Sensitivity",1);
    }

    void OnEnable()
    {
        var tmp = GameObject.FindObjectOfType<CollectibleGuiController>();
        if (null != tmp)
        {
            tmp.ShowGui(false);
        }
    }

    private void OnDisable()
    {
        var tmp = GameObject.FindObjectOfType<CollectibleGuiController>();
        var gm = GameObject.FindObjectOfType<GameManager>();
        
        if (null != tmp && gm != null && gm.ShowCollectiblesTimed)
        {
            tmp.HideGui();
        }
    }

    private void initSlider(Slider slider, string mixerName)
    {
        SoundManager.Instance.AudioMixer.GetFloat(mixerName, out var newMixerVolume);
        slider.value = SoundManager.dbToLinear(newMixerVolume);

    }
    private void handleMouseSettings(Slider slider, float newValue)
    {
        SettingsManager.Instance.SetMouseSensitivity(newValue);
    }

    void handleSoundSettings(Slider slider,float newValue)
    {
        switch (slider)
        {
            case var main when main == mainVolumeSlider:
                SoundManager.Instance.SetMainVolume(newValue);
                break;
            case var music when music == musicVolumeSlider:
                SoundManager.Instance.SetMusicVolume(newValue);
                break;
            case var voice when voice == voiceVolumeSlider:
                SoundManager.Instance.SetVoiceVolume(newValue);
                break;
            case var sound when sound == soundeffectsVolumeSlider:
                SoundManager.Instance.SetSoundEffectVolume(newValue);
                break;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
