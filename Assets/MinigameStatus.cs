using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameStatus : MonoBehaviour
{

    public bool isDone = false;
    private StoryTrigger _storyTrigger;
    
    // Start is called before the first frame update
    void Start()
    {
        _storyTrigger = GetComponent<StoryTrigger>();
    }

    // Update is called once per frame
    void Update()
    {
        
        //if true irgendwas tun?
    }

    public void SetDone()
    {
        isDone = true;
        
        if (_storyTrigger != null)
        {
            _storyTrigger.Activate();
        }
    }
}
