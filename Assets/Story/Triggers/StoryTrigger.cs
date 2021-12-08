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

    public void Activate()
    {
        if (!_marker.AccomplishedMarker)
        {
            _marker.AccomplishedMarker = true;
            EventManager.TriggerEvent($"marker_{_marker.IndexInStory}", _marker);
            EventManager.TriggerEvent(StoryManager.evt_StoryMarkerActivated, _marker);
        }
    }
}
