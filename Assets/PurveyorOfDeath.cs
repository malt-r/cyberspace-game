using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PurveyorOfDeath : MonoBehaviour
{
    [SerializeField] 
    private string EventNameOnDeath;
    
    private BoxCollider _collider;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        var combatant = other.gameObject.GetComponent<CombatParticipant>();
        if (combatant != null)
        {
            combatant.TakeDamage(float.MaxValue);

            StoryEventData data = new StoryEventData().SetEventName(EventNameOnDeath);
            EventManager.TriggerEvent(EventNameOnDeath, data);
        }
    }
}
