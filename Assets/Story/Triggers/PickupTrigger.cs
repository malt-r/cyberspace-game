using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupTrigger : StoryTrigger
{
    [SerializeField]
    BaseItem ItemToWatch;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        ItemToWatch.RegisterPickupCallback(OnPickup);
        base.InitMarker();
    }

    private void OnDisable()
    {
        ItemToWatch.UnregisterPickupCallback(OnPickup);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPickup()
    {
        // item was picked up
        Activate();
    }
}
