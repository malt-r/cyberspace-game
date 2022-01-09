using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BossMusicManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup outputGroup;

    [SerializeField] private int crossFadeTime = 20;

    [SerializeField] private AudioClip droneClip;
    [SerializeField] private AudioClip transitionClip;
    [SerializeField] private AudioClip actionClip;
    
    [SerializeField] private float volumeDrone;
    [SerializeField] private float volumeTransition;
    [SerializeField] private float volumeAction;

    [SerializeField] private bool transToAction;
    [SerializeField] private bool transToDrone;
    [SerializeField] private bool transToTransition;

    public enum TrackType
    {
        none,
        drone,
        transition,
        action
    }

    public struct TrackInfo
    {
        public AudioClip clip;
        public float volume;
    }

    public int CrossFadeTime => crossFadeTime;

    public TrackInfo GetCorrespondingAudioClip(TrackType trackType)
    {
        var info = new TrackInfo();
        switch (trackType)
        {
            case TrackType.drone:
                info.clip = droneClip;
                info.volume = volumeDrone;
                break;
            case TrackType.transition:
                info.clip = transitionClip;
                info.volume = volumeTransition;
                break;
            case TrackType.action:
                info.clip = actionClip;
                info.volume = volumeAction;
                break;
        }

        return info;
    }
    
   //We create an array with 2 audio sources that we will swap between for transitions
    public static AudioSource[] aud = new AudioSource[2];
    //We will use this boolean to determine which audio source is the current one
    bool activeMusicSource;
    //We will store the transition as a Coroutine so that we have the ability to stop it halfway if necessary
    IEnumerator musicTransition;
 
    void Awake () {
        //Create the AudioSource components that we will be using
        aud[0] = gameObject.AddComponent<AudioSource>();
        aud[1] = gameObject.AddComponent<AudioSource>();
        
        aud[0].outputAudioMixerGroup = outputGroup;
        aud[1].outputAudioMixerGroup = outputGroup;

        aud[0].loop = true;
        aud[1].loop = true;
    }
 
    //use this method to start a new soundtrack, with a reference to the AudioClip that you want to use
    //    such as:        newSoundtrack((AudioClip)Resources.Load("Audio/soundtracks/track01"));
    public void newSoundtrack (TrackType type) {
        //This ?: operator is short hand for an if/else statement, eg.
        //
        //      if (activeMusicSource) {
        //          nextSource = 1;
        //      } else {
        //           nextSource = 0;
        //      }
 
        int nextSource = !activeMusicSource ? 0 : 1;
        int currentSource = activeMusicSource ? 0 : 1;

        var info = GetCorrespondingAudioClip(type);
 
        //If the clip is already being played on the current audio source, we will end now and prevent the transition
        if (info.clip == aud[currentSource].clip)
            return;
 
        //If a transition is already happening, we stop it here to prevent our new Coroutine from competing
        if (musicTransition != null)
            StopCoroutine(musicTransition);
 
        aud[nextSource].clip = info.clip;
        aud[nextSource].Play();
 
        musicTransition = transition(crossFadeTime, info.volume); //20 is the equivalent to 2 seconds (More than 3 seconds begins to overlap for a bit too long)
        StartCoroutine(musicTransition);
    }
 
        //  'transitionDuration' is how many tenths of a second it will take, eg, 10 would be equal to 1 second
    IEnumerator transition(int transitionDuration, float volume) {
 
        for (int i = 0; i < transitionDuration+1; i++) {
            aud[0].volume = activeMusicSource ? (transitionDuration - i) * (1f / transitionDuration) : (0 + i) * (1f / transitionDuration);
            aud[1].volume = !activeMusicSource ? (transitionDuration - i) * (1f / transitionDuration) : (0 + i) * (1f / transitionDuration);
 
            //  Here I have a global variable to control maximum volume.
            //  options.musicVolume is a float that ranges from 0f - 1.0f
            //------------------------------------------------------------//
            //aud[0].volume *= options.musicVolume;
            //aud[1].volume *= options.musicVolume;
            aud[0].volume *= volume;
            aud[1].volume *= volume;
            //------------------------------------------------------------//
 
            yield return new WaitForSecondsRealtime(0.1f);
            //use realtime otherwise if you pause the game you could pause the transition half way
        }
 
        //finish by stopping the audio clip on the now silent audio source
        aud[activeMusicSource ? 0 : 1].Stop();
 
        activeMusicSource = !activeMusicSource;
        musicTransition = null;
    } 
    
    
    // Start is called before the first frame update
    void Start()
    {
        newSoundtrack(TrackType.drone);
        
        //OVERRIDE AUDIO MANAGER
        FindObjectOfType<SoundManager>().StopBackgroundMusic();
    }

    // Update is called once per frame
    void Update()
    {
        if (transToAction)
        {
            Debug.Log("To Action");
            newSoundtrack(TrackType.transition);
            transToAction = false;
        }

        if (transToDrone)
        {
            Debug.Log("To Drone");
            newSoundtrack(TrackType.drone);
            transToDrone = false;
        }
        
        if (transToTransition)
        {
            Debug.Log("To Transition");
            newSoundtrack(TrackType.transition);
            transToTransition = false;
        }
    }
}
