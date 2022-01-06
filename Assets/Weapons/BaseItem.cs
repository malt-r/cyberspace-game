using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseItem : MonoBehaviour
{
    //List<UnityAction> pickupCallbacks = new List<UnityAction>();
    UnityEvent pickupEvent = new UnityEvent();

    public bool Absorbable = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void Visit(Transform player);
    public void RegisterPickupCallback(UnityAction action)
    {
        pickupEvent.AddListener(action);
        //pickupCallbacks.Add(action);
    }

    public void UnregisterPickupCallback(UnityAction action)
    {
        pickupEvent.RemoveListener(action);
    }

    protected void OnPickup()
    {
        pickupEvent.Invoke();
    }
}
