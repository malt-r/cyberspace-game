using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// TODO:
// only story markers with idx < 0 are to be placed wherever, indices greater 0 are still relevant for the level strucure, but not for the
// story

// TODO: 
// - integrate with tutorial manager
// - create triggers and events for each relevant story thingy
// - create event per trigger
public class StoryEventData
{
    private string _eventName;
    private StoryMarker _marker;
    private object _sender;

    public StoryMarker Marker => _marker;
    public string EventName => _eventName;
    public object Sender => _sender;

    public StoryEventData SetEventName(string name)
    {
        _eventName = name;
        return this;
    }
    
    public StoryEventData SetMarker(StoryMarker marker)
    {
        _marker = marker;
        return this;
    }

    public StoryEventData SetSender(object sender)
    {
        _sender = sender;
        return this;
    }
}

public class StoryManager : MonoBehaviour
{
    
    
    public const string evt_StoryMarkerActivated = "StoryMarkerActivated";

    private StoryMarker _lastFinishedStoryMarker;
    private StoryMarker _currentStoryMarker;
    private bool _readRooms;
    private Dictionary<int, List<StoryMarker>> _storyMarkers;
    private int[] _markerIdxs;
    private int _sequentialMarkerIdx; // index into _markerIdxs, which is used to index into _storyMarkers

    private StoryState _currentState;
    private AudioSource _audioSource;
    
    [SerializeField] private TMP_Text storyTextlabel;

    public StoryMarker CurrentStoryMarker
    {
        get => _currentStoryMarker;
    } 

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        
        // This would be a good place to create more modularity to enable hooking in external handlers for story events
        EventManager.StartListening(evt_StoryMarkerActivated, StoryMarkerActivated);
        EventManager.StartListening("Level/EndLevel", FinishLevel);

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
    }

    private void StoryMarkerActivated(object data)
    {
        var eventData = data as StoryEventData;
        var marker = eventData.Marker;
        
        Debug.Log($"Story Marker {marker.IndexInStory} activated");
        _lastFinishedStoryMarker = marker;

        // find next one.. it's possible, that this is not in sequential order..
        // this could be done with yet another dictionary, but realistically we won't have more than 30 markers or so, so 
        // a little bit of linear time won't hurt
        // TODO: account for story markers, which's index is not -1 but which are also not relevant for the wayfinder
        
        // ignore marker, if index in story is lower than currently active index
        if (marker.IndexInStory < _currentStoryMarker.IndexInStory)
        {
            return;
        }

        for (int i = 0; i < _markerIdxs.Length; i++)
        {
            if (_markerIdxs[i] == marker.IndexInStory)
            {
                // skip marker, which's ShowAsStoryTask-flag is not set
                _sequentialMarkerIdx = i + 1;
                var nextPotentialMarker = _storyMarkers[_markerIdxs[_sequentialMarkerIdx]].First();
                while (!nextPotentialMarker.ShowAsStoryTask)
                {
                    _sequentialMarkerIdx++;
                    nextPotentialMarker = _storyMarkers[_markerIdxs[_sequentialMarkerIdx]].First();
                }

                _currentStoryMarker = nextPotentialMarker;
            }
        }
        //_currentStoryMarker = _storyMarkers[_markerIdxs[_sequentialMarkerIdx]].First();
        
        UpdateStoryUI();
    }

    private void UpdateStoryUI()
    {
        // update task description in UI
        storyTextlabel = GameObject.Find("StoryUI").transform.Find("Task").GetComponent<TMP_Text>();
        storyTextlabel.text = _currentStoryMarker.Description;
        
        GameObject.Find("StoryUI").transform.Find("Task").GetComponent<Animator>()
            .SetTrigger("TriggerAnimation");
    }

    public static void UpdateStoryUI(string text)
    {
        // update task description in UI
        var storyTextlabel = GameObject.Find("StoryUI").transform.Find("Task").GetComponent<TMP_Text>();
        storyTextlabel.text = text;
        
        GameObject.Find("StoryUI").transform.Find("Task").GetComponent<Animator>()
            .SetTrigger("TriggerAnimation");
    }

    private void FinishLevel(object data)
    {
        Debug.Log("Finishing the Level");
    }

    private bool ReadRooms()
    {
        var gen = FindObjectOfType<DungeonGenerator>();
        if (!gen.FinishedGenerating) return false;
        var instRooms = gen.InstantiatedRooms;

        Dictionary<int, List<StoryMarker>> storyMarkers = new Dictionary<int, List<StoryMarker>>();
        foreach (var room in instRooms)
        {
            foreach (var marker in room.Value.GetStoryMarkers())
            {
                if (storyMarkers.TryGetValue(marker.IndexInStory, out var listForIdx))
                {
                    listForIdx.Add(marker);
                }
                else
                {
                    storyMarkers.Add(marker.IndexInStory, new List<StoryMarker>() {marker});
                }
            }
        }
        
        _storyMarkers = storyMarkers;
        _markerIdxs = _storyMarkers.Keys.ToArray();
        _markerIdxs = _markerIdxs.OrderBy(elem => elem).ToArray();
        
        // filter out non-relevant story markers
        _sequentialMarkerIdx = 0;
        if (_markerIdxs.First() == -1)
        {
            _sequentialMarkerIdx = 1;
        }

        _currentStoryMarker = _storyMarkers[_markerIdxs[_sequentialMarkerIdx]].First();
        return true;
}
    
    // Update is called once per frame
    void Update()
    {
        if (!_readRooms)
        {
            _readRooms = ReadRooms();
            if (_readRooms)
            {
                Console.WriteLine("Read rooms");
                UpdateStoryUI();
            }
        }
    }
}
