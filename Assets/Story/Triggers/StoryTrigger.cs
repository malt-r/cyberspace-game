using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryTrigger : MonoBehaviour
{
    StoryMarker _marker;

    [SerializeField]
    string storyEventName;

    [SerializeField] 
    private StoryMarker marker;

    [SerializeField] 
    private bool ActivateOnlyOnce = true;

    public Action OnActivateTrigger;

    private bool _wasActivated;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    protected void InitMarker()
    {
        if (marker != null)
        {
            _marker = marker;
        }
        else
        {
            _marker = GetComponentInParent<StoryMarker>();
        }
        
        if (_marker == null)
        {
            Debug.LogWarning("Found no story marker");
        }
    }

    private void OnEnable()
    {
        InitMarker();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public StoryEventData CreateEventData(string eventName)
    {
        StoryEventData data = new StoryEventData();
        if (_marker != null)
        {
            data.SetMarker(_marker);
        }
        data.SetEventName(eventName);
        return data;
    }

    public void Activate()
    {
        if (!_wasActivated)
        {
            if (_marker != null && !_marker.AccomplishedMarker)
            {
                _marker.AccomplishedMarker = true;
                
                // standard event for story progression
                var storyData = CreateEventData(StoryManager.evt_StoryMarkerActivated);
                EventManager.TriggerEvent(StoryManager.evt_StoryMarkerActivated, storyData);
            }
            
            // custom event for triggering of specific response
            if (!string.IsNullOrEmpty(storyEventName))
            {
                var data = CreateEventData(storyEventName);
                EventManager.TriggerEvent(storyEventName, data);
            }

            if (OnActivateTrigger != null)
            {
                OnActivateTrigger.Invoke();
            }
            
            // remember activation
            if (ActivateOnlyOnce)
            {
                _wasActivated = true;
            }
        }
    }
}
