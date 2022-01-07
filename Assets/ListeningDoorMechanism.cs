using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListeningDoorMechanism : Doormechanism
{
    [SerializeField] 
    private StoryTrigger triggerToListenTo;
    // Start is called before the first frame update
    void Start()
    {
        if (triggerToListenTo != null)
        {
            triggerToListenTo.OnActivateTrigger += () => openDoors();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
