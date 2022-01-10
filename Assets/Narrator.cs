using System;
using Random = UnityEngine.Random;
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

    private Dictionary<string, List<EventReactionClip>> _reactionClipDict;

    private AudioSource _audioSource;

    private bool canNarrate = true;
    // Start is called before the first frame update
    void Start()
    {
    }

    void OnEnable()
    {
        _reactionClipDict = new Dictionary<string, List<EventReactionClip>>();
        // build dict for reactions 
        foreach (var reaction in eventReactions)
        {
            if (!_reactionClipDict.TryGetValue(reaction.eventName, out var reactionList))
            {
                _reactionClipDict.Add(reaction.eventName, new List<EventReactionClip>(){reaction});
            }
            else
            {
                reactionList.Add(reaction);
            }
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
        if (!canNarrate) { return; }
        
        var eventData = data as StoryEventData;
        var eventName = eventData.EventName;
        if (_reactionClipDict.TryGetValue(eventName, out List<EventReactionClip> reactions))
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            AudioClip clip;
            // select clip
            if (reactions.Count == 1)
            {
                clip = reactions[0].clip;
            }
            else
            {
                // pick random one
                int idx = Random.Range(0, reactions.Count);
                clip = reactions[idx].clip;
            }
            _audioSource.PlayOneShot(clip);
            
            if (eventData.Sender is TutorialManager)
            {
                var length = clip.length;
                Invoke("SignalTutorialReadyForNextStage", length * tutorialDelayAddition);
            }
        }
        else
        {
            Debug.LogError("I don't know how to react to an event like " + eventName);
        }

        if (eventName == "Boss/Death")
        {
            canNarrate = false;
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
