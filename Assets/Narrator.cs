using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    [Serializable]
    struct EventReactionClip
    {
        public string eventName;
        public AudioClip clip;
    }
    
    [SerializeField] 
    private EventReactionClip[] eventReactions;

    [SerializeField] private float tutorialDelayAddition = 1.0f;

    private Dictionary<string, EventReactionClip> _reactionClipDict;

    private AudioSource _audioSource;
    // Start is called before the first frame update
    void Start()
    {
    }

    void OnEnable()
    {
        _reactionClipDict = new Dictionary<string, EventReactionClip>();
        // build dict for reactions 
        foreach (var reaction in eventReactions)
        {
            _reactionClipDict.Add(reaction.eventName, reaction);
        }

        _audioSource = GetComponent<AudioSource>();
        foreach (var eventName in _reactionClipDict.Keys)
        {
            EventManager.StartListening(eventName, EventReaction);
        }
    }

    void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EventReaction(object data)
    {
        var eventData = data as StoryEventData;
        var eventName = eventData.EventName;
        if (_reactionClipDict.TryGetValue(eventName, out EventReactionClip reaction))
        {
            _audioSource.PlayOneShot(reaction.clip);
            if (eventData.Sender is TutorialManager)
            {
                var length = reaction.clip.length;
                Invoke("SignalTutorialReadyForNextStage", length * tutorialDelayAddition);
            }
        }
        else
        {
            Debug.LogError("I don't know how to react to an event like " + eventName);
        }
    }

    void SignalTutorialReadyForNextStage()
    {
        var man = FindObjectOfType<TutorialManager>();
        if (man != null)
        {
            man.SignalReadyForNextStage();
        }
    }
}
