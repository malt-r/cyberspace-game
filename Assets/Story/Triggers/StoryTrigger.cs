using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    StoryMarker _marker;

    [SerializeField]
    string storyEventName;

    [SerializeField] 
    private StoryMarker marker;

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
        data
            .SetMarker(_marker)
            .SetEventName(eventName);
        return data;
    }

    public void Activate()
    {
        if (!_marker.AccomplishedMarker)
        {
            _marker.AccomplishedMarker = true;
            
            // standard event for story progression
            var data = CreateEventData(StoryManager.evt_StoryMarkerActivated);
            EventManager.TriggerEvent(StoryManager.evt_StoryMarkerActivated, data);
            
            // custom event for triggering of specific response
            data = CreateEventData(storyEventName);
            EventManager.TriggerEvent(storyEventName, data);
        }
    }
}
