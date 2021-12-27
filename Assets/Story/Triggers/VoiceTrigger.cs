using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceTrigger : MonoBehaviour
{
    [SerializeField]
    string eventName;

    private bool _wasActivated;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public StoryEventData CreateEventData(string eventName)
    {
        StoryEventData data = new StoryEventData();
        data
            //.SetMarker(_marker)
            .SetEventName(eventName);
        return data;
    }

    public void Activate()
    {
        if (!_wasActivated)
        {
            _wasActivated = true;
            
            // custom event for triggering of specific response
            var data = CreateEventData(eventName);
            EventManager.TriggerEvent(eventName, data);
        }
    }
}