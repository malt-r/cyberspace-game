using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public const string evt_StoryMarkerActivated = "StoryMarkerActivated";
    // TOOD: figure this out
    // - what to do, what to do?
    // - 

    private StoryState _currentState;
    private AudioSource _audioSource;


    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        EventManager.StartListening(evt_StoryMarkerActivated, StoryMarkerActivated);
        EventManager.StartListening("marker_foundLaser", FoundLaser);

        _audioSource = this.GetComponent<AudioSource>();
        if (null == _audioSource)
        {
            Debug.Log("Audio Source is null");
        }
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        EventManager.StopListening(evt_StoryMarkerActivated, StoryMarkerActivated);
        EventManager.StopListening("marker_foundLaser", FoundLaser);
    }

    private void StoryMarkerActivated(object data)
    {
        var marker = data as StoryMarker;
        Debug.Log($"Story Marker {marker.IndexInStory} activated");

        if (marker.audioClip != null)
        {
            _audioSource.Stop();
            _audioSource.PlayOneShot(marker.audioClip);
        }
    }

    private void FoundLaser(object data)
    {
        Debug.Log("Found the Lazor");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
