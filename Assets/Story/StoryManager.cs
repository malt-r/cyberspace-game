using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    // TODO: What does this have to accomplish?
    // - keep track of the current 'state' of the story
    // - advance the story based on 'events', which can be
    // triggered by some actions of the player (e.g. entering a room, solving a puzzle)
    // - keep track of specific data to advance the state further (should be done in state machine)
    // - make the current state accessible for use by the 'speech-manager'

    [SerializeField]
    StoryState[] states;

    private StoryState _currentState;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
