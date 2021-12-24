using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    StoryMarker _marker;

    [Header("THE EVENT NAME WILL BE PREFIXED WITH 'marker_'")]
    [SerializeField]
    string eventName;

    // Start is called before the first frame update
    void Start()
    {

    }

    protected void InitMarker()
    {
        _marker = GetComponentInParent<StoryMarker>();
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
            .SetEventName(StoryManager.evt_StoryMarkerActivated);
        return data;
    }

    public void Activate()
    {
        if (!_marker.AccomplishedMarker)
        {
            _marker.AccomplishedMarker = true;
            var data = CreateEventData(StoryManager.evt_StoryMarkerActivated);
            EventManager.TriggerEvent(StoryManager.evt_StoryMarkerActivated, data);
            //EventManager.TriggerEvent($"marker_{_marker.IndexInStory}", _marker);
        }
    }
}
